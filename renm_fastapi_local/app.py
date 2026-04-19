from __future__ import annotations

import os
import re
import shutil
from typing import List, Dict, Any
from fastapi import FastAPI, HTTPException, Request
from fastapi.responses import HTMLResponse
from fastapi.templating import Jinja2Templates
from pydantic import BaseModel, Field

app = FastAPI(title="Local Rename Tool")
templates = Jinja2Templates(directory="templates")


# -------------------------
# Models
# -------------------------
class ListRequest(BaseModel):
    dir: str = Field(..., description="Target directory path")


class RenameRequest(BaseModel):
    dir: str
    before: str
    after: str
    recursive: bool = True
    include_hidden: bool = False


class UndoRequest(BaseModel):
    dir: str


# -------------------------
# Helpers
# -------------------------
def _is_hidden_name(name: str) -> bool:
    # ざっくり判定: Unix系はドット始まり
    return name.startswith(".")


def ensure_backup(dir_path: str) -> str:
    """Create/refresh backup directory: <dir>.bk"""
    if not os.path.isdir(dir_path):
        raise FileNotFoundError(f"Directory not found: {dir_path}")

    undo_dir = dir_path + ".bk"

    # 既存バックアップがあれば作り直す
    if os.path.isdir(undo_dir):
        shutil.rmtree(undo_dir)

    shutil.copytree(dir_path, undo_dir)
    return undo_dir


def list_dir(dir_path: str, include_hidden: bool) -> Dict[str, Any]:
    if not os.path.isdir(dir_path):
        raise FileNotFoundError(f"Directory not found: {dir_path}")

    items: List[Dict[str, Any]] = []
    with os.scandir(dir_path) as it:
        for entry in it:
            if not include_hidden and _is_hidden_name(entry.name):
                continue
            items.append(
                {
                    "name": entry.name,
                    "is_dir": entry.is_dir(),
                    "is_file": entry.is_file(),
                }
            )
    items.sort(key=lambda x: (not x["is_dir"], x["name"].lower()))
    return {"dir": dir_path, "items": items}


def rename_walk(dir_path: str, before: str, after: str, recursive: bool, include_hidden: bool) -> Dict[str, Any]:
    if not os.path.isdir(dir_path):
        raise FileNotFoundError(f"Directory not found: {dir_path}")

    # 正規表現としてコンパイル（beforeが不正ならここで例外）
    pattern = re.compile(before)

    logs: List[str] = []
    changes: List[Dict[str, str]] = []

    def _walk(cur: str, indent: str = "") -> None:
        logs.append(f"{indent}=> {cur}")

        # entryを先にリスト化（リネームしながらscandirを回すと不安定なことがあるため）
        entries = list(os.scandir(cur))

        for entry in entries:
            name = entry.name
            if not include_hidden and _is_hidden_name(name):
                continue

            src = os.path.join(cur, name)

            # 再帰（ディレクトリは中身を先に処理）
            if entry.is_dir(follow_symlinks=False) and recursive:
                _walk(src, indent + "    ")

            # ファイル名の拡張子は維持（最後の . で分割）
            if "." in name:
                base, ext = name.rsplit(".", 1)
                ext = "." + ext
            else:
                base, ext = name, ""

            new_base = pattern.sub(after, base)
            dst = os.path.join(cur, new_base + ext)

            # 変化なし
            if src == dst:
                continue

            # 既に存在するなら事故防止で停止
            if os.path.exists(dst):
                raise FileExistsError(f"Destination exists: {dst}")

            logs.append(f"{indent}    {src}")
            logs.append(f"{indent}    -> {dst}")

            os.rename(src, dst)
            changes.append({"from": src, "to": dst})

        logs.append(f"{indent}<= {cur}")

    _walk(dir_path)
    return {"logs": logs, "changes": changes}


def undo(dir_path: str) -> Dict[str, Any]:
    undo_dir = dir_path + ".bk"
    if not os.path.isdir(undo_dir):
        raise FileNotFoundError("Backup (.bk) not found")

    # 復元：現行を削除→バックアップを戻す
    shutil.rmtree(dir_path)
    os.rename(undo_dir, dir_path)
    return {"message": "recovered"}


# -------------------------
# Routes (UI)
# -------------------------
@app.get("/", response_class=HTMLResponse)
def index(request: Request):
    return templates.TemplateResponse("index.html", {"request": request})


# -------------------------
# Routes (API)
# -------------------------
@app.post("/api/list")
def api_list(req: ListRequest):
    try:
        return {"ok": True, **list_dir(req.dir, include_hidden=False)}
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.post("/api/rename")
def api_rename(req: RenameRequest):
    try:
        ensure_backup(req.dir)
        result = rename_walk(req.dir, req.before, req.after, req.recursive, req.include_hidden)
        return {"ok": True, **result}
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.post("/api/undo")
def api_undo(req: UndoRequest):
    try:
        result = undo(req.dir)
        return {"ok": True, **result}
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

