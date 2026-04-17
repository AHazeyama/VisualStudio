# -*- coding: utf-8 -*-

#┌─────────────────────────────
#│ Name      : hashgc_fastapi.py
#│ FrameWort : FastAPI
#│ Function  : Hash value generation and comparison tool
#└─────────────────────────────

# app.py
from __future__ import annotations

import hashlib
import os
from pathlib import Path
from typing import Literal, Optional

from fastapi import FastAPI, File, Form, UploadFile, Request
from fastapi.responses import HTMLResponse
from fastapi.templating import Jinja2Templates

app = FastAPI()
templates = Jinja2Templates(directory="templates")

Algo = Literal["md5", "sha1", "sha3_256", "sha256", "sha512", "blake2b"]

def hash_file(path: Path, algo: Algo) -> str:
    if algo == "md5":
        h = hashlib.md5()
    elif algo == "sha1":
        h = hashlib.sha1()
    elif algo == "sha3_256":
        h = hashlib.sha3_256()
    elif algo == "sha256":
        h = hashlib.sha256()
    elif algo == "sha512":
        h = hashlib.sha512()
    elif algo == "blake2b":
        h = hashlib.blake2b()
    else:
        raise ValueError(f"Unknown algo: {algo}")

    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


@app.get("/", response_class=HTMLResponse)
async def index(request: Request):
    return templates.TemplateResponse(
        request,
        "index.html",
        {
            "result": None,
        },
    )

@app.post("/check", response_class=HTMLResponse)
async def check(
    request: Request,
    algo: Algo = Form("sha256"),
    expected: str = Form(""),
    file: UploadFile = File(...),
):
    # アップロードを一時保存（デプロイ時は /tmp 相当を使う想定）
    tmp_dir = Path(os.environ.get("TMPDIR") or os.environ.get("TEMP") or "/tmp")
    tmp_dir.mkdir(parents=True, exist_ok=True)
    tmp_path = tmp_dir / f"hashgc_{os.getpid()}_{file.filename}"

    try:
        # 大きいファイルでもOK（ストリーム書き込み）
        with tmp_path.open("wb") as w:
            while True:
                chunk = await file.read(1024 * 1024)
                if not chunk:
                    break
                w.write(chunk)

        generated = hash_file(tmp_path, algo)
        expected_s = (expected or "").strip()
        match: Optional[bool] = None
        if expected_s:
            match = (expected_s == generated)

        return templates.TemplateResponse(
            request,
            "index.html",
            {
                "result": {
                    "filename": file.filename,
                    "algo": algo,
                    "generated": generated,
                    "expected": expected_s,
                    "match": match,
                },
            },
        )
    finally:
        try:
            tmp_path.unlink(missing_ok=True)
        except Exception:
            pass

