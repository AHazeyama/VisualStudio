<p align="left">
  <img src="./image/Title_dark.png#gh-dark-mode-only" alt="banner dark">
  <img src="./image/Title_light.png#gh-light-mode-only" alt="banner light">
</p>

# Hash value generation and comparison tool [hashgc]
![](./image/hashgc00.png)

<br>

## Overview
ファイルの整合性確認（チェックサム検証）を手作業で行う際の手間とミスを削減するために開発したツールです。  

複数のハッシュアルゴリズムに対応し、生成結果と期待値の比較をワンステップで実行可能です。  
業務における検証作業の効率化およびヒューマンエラー防止を目的としています。

また、FastAPI により API 化することで、ローカル利用に加えて  
将来的な自動化処理やクラウド環境での利用も視野に入れた構成としています。

<br>

## Features
- ファイルからハッシュ値を生成
- 期待値との比較（Match / Discrepancy 表示）
- 複数アルゴリズム対応  
  - MD5 / SHA-1 / SHA-256 / SHA-512 / BLAKE2
- クリップボードから期待値を貼り付け（Paste）
- シンプルなUIによる直感的操作
- エラーハンドリング（未選択・不正入力）
- メッセージ表示による操作ガイド

<br>

## API (FastAPI)
本ツールは FastAPI により API 化されています。
### Endpoints
- `POST /hash`
  - ファイルをアップロードしてハッシュ値を生成
- `POST /compare`
  - 生成したハッシュと期待値を比較

<br>

## Usage
1. 「Select」で対象ファイルを選択  
   またはドラッグ&ドロップでアップロード  
2. ハッシュアルゴリズムを選択  
3. 期待値を貼り付け（任意）  
4. 「Check」をクリック  

<br>

## Use Case
- ダウンロードファイルの整合性確認
- 配布物の改ざん検知（内容保証）
- 検証作業の自動化前段階としての利用

<br>

## Tech Stack
- Python 3.x
- FastAPI
- Jinja2
- python-multipart

<br>

## Requirements
- Python 3.10 以上
- pip

<br>

## Install
![](./image/shell_logo.png)
```bash
pip install -r requirements.txt
```

<br>

## Run
![](./image/shell_logo.png)
```bash
uvicorn app:app --host 0.0.0.0 --port 8000 --reloaduvicorn app:app --host 0.0.0.0 --port 8000 --reload
```

<br>

## Access
* Application  
    http://localhost:8000
* Swagger UI  
http://localhost:8000/docs

<br>

## Run with Docker
Docker を使用して実行することもできます。  

<br>

## Build
![](./image/bash_logo.png)
```bash
docker build -t hashgc .
```

<br>

## Run
![](./image/bash_logo.png)
```bash
docker run -p 8000:8000 hashgc
```

## Access
* Application  
  http://localhost:8000  
* Swagger UI  
  http://localhost:8000/docs

<br>

## Deployment Perspective
本ツールは単体のローカルGUI用途に留まらず、FastAPI による API 化により、以下のような運用を想定しています。  
* ローカル環境での検証ツール
* 社内向けAPIとしての利用
* Docker コンテナによる実行環境の統一
* クラウド環境への展開

<br>

## Documentation
Doxygen により生成できます。
ソースコードの可読性向上と構造理解を目的としています。  
![](./image/bash_logo.png)
```bash
doxygen Doxyfile
```
生成後、以下をブラウザで開くことでドキュメントを確認できます。  
📁 **/** 📄
```
docs/html/index.html
```

<br>

## License
TBD