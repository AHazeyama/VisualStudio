<p align="left">
  <img src="./image/Title_dark.png#gh-dark-mode-only" alt="banner dark">
  <img src="./image/Title_light.png#gh-light-mode-only" alt="banner light">
</p>

# Exclusive removal tool [exrm]

![](./image/View00.png)

---

## Overview

指定した文字列を**含まない**ファイル / ディレクトリを一括削除するためのデスクトップツールです。

大量のファイルやディレクトリを手作業で整理する際、削除対象の選別には時間がかかり、操作ミスも発生しやすくなります。  
本ツールはそのような作業を効率化するために開発しました。

c# / WPF により実装し、Windows 環境で直感的に利用できる GUI ツールとして構成しています。

---

## Download

� https://github.com/AHazeyama/public/releases/latest  

---

## Purpose

- 不要ファイル整理の手間を削減する
- 手作業による選別漏れや削除ミスを減らす
- 大量ファイルを対象とした反復作業を効率化する
- 条件付き削除を GUI で簡単に実行できるようにする

---

## Features

- 指定ディレクトリ配下のファイル / ディレクトリを一括処理
- 指定文字列を**含まない**対象を削除
- 再帰処理対応
- 処理内容を画面上に表示
- Undo 機能を搭載
- WPF によるデスクトップ UI
- 単体実行しやすい Windows 向けツール

---

## Caution

本ツールは削除系ツールのため、誤操作時の影響が大きくなり得ます。  
そのため、以下を意識して設計しています。

- 条件を明示して実行できる UI
- 処理内容の可視化
- Undo による復元支援
- 注意喚起メッセージの表示

※ 実運用では、重要データに対して使用する前にテスト用ディレクトリでの確認を推奨します。

---

## Main Screen

### Input items

| Item | Description |
|:--|:--|
| Exec directory | 処理対象ディレクトリ |
| not removed words | 削除対象外とする文字列 |
| Recursive processing | 下位ディレクトリを含めて再帰処理 |
| Processing message | 処理内容・注意事項の表示 |

### Buttons

| Button | Description |
|:--|:--|
| ExRemove | 削除実行 |
| Clear | 入力内容の初期化 |
| Undo | 直前状態への復元支援 |
| Help | 操作方法表示 |
| Exit | アプリ終了 |

---

## Use Case

- 作業フォルダ内の不要ファイル整理
- 一部のキーワードを含む成果物だけを残したい場合
- ビルド生成物や中間ファイルの整理
- 大量の検証ファイルから必要なものだけを残す作業

---

## Tech Stack

- C#
- WPF
- .NET
- Visual Studio

---

## Design / Implementation Points

- 単なる削除ツールではなく、**条件付き一括削除**に特化
- GUI から扱えるようにして、CLI に不慣れな利用者でも操作可能
- 危険な処理であるため、画面上に注意メッセージを明示
- Undo を実装し、操作リスクの軽減を意識
- 処理メッセージ表示により、何が起きているかを分かりやすく可視化

---

## Why WPF

本ツールは Windows デスクトップ上でのファイル整理作業を想定しているため、  
gUI 操作との相性が良い WPF を採用しています。

- Windows ネイティブに近い操作感
- 業務ツールとして実装しやすい
- 状態表示やメッセージ表示を組み込みやすい
- デスクトップユーティリティに適した構成

## Documentation  

Doxygen により生成できます。
→ ソースコードの可読性向上と構造理解を目的としています。
```bash  
doxygen Doxyfile
```

---

## 📄 License

TBD

---