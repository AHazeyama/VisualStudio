# -*- coding: utf-8 -*-

#┌───────────────────────────────────────────────────────
#│ Name      : ptnc.py
#│ FrameWort : FLASK
#│ Function  : Positional numeral conversion tool
#└───────────────────────────────────────────────────────

from flask import Flask, render_template, request, jsonify
import re

app = Flask(__name__)


# =========================
# Validate
# =========================

def is_bin(value: str) -> bool:
    return re.fullmatch(r"[01,]*", value or "") is not None


def is_oct(value: str) -> bool:
    return re.fullmatch(r"[0-7,]*", value or "") is not None


def is_dec(value: str) -> bool:
    return re.fullmatch(r"-?[0-9]*", value or "") is not None


def is_hex(value: str) -> bool:
    return re.fullmatch(r"[0-9a-fA-F,]*", value or "") is not None


def is_digit(value: str) -> bool:
    return re.fullmatch(r"[0-9]*", value or "") is not None


# =========================
# Base -> Decimal
# 計算で変換
# =========================

def base_to_dec(value_s: str, base: int) -> int:
    value_s = value_s.replace(",", "").upper()

    if value_s == "":
        raise ValueError("入力値が空です。")

    table = "0123456789ABCDEF"
    dec_d = 0
    power = 0

    for c in reversed(value_s):
        digit = table.find(c)

        if digit < 0 or digit >= base:
            raise ValueError("入力値が不正です。")

        dec_d += digit * (base ** power)
        power += 1

    return dec_d


# =========================
# Decimal -> Base
# 計算で変換
# =========================

def dec_to_base(value_d: int, base: int) -> str:
    table = "0123456789ABCDEF"

    if value_d == 0:
        return "0"

    result = ""

    while value_d >= 1:
        result = table[value_d % base] + result
        value_d //= base

    return result


# =========================
# Width / Padding
# =========================

def pad_left(value_s: str, width: int, zero_char: str = "0") -> str:
    while len(value_s) < width:
        value_s = zero_char + value_s

    return value_s


def group_text(value_s: str, group_size: int) -> str:
    result = ""
    count = 0

    for c in reversed(value_s):
        if count > 0 and count % group_size == 0:
            result = "," + result

        result = c + result
        count += 1

    return result


def required_digits(bit_width: int, base: int) -> int:
    if base == 2:
        return bit_width

    if base == 8:
        return (bit_width + 2) // 3

    if base == 16:
        return (bit_width + 3) // 4

    raise ValueError("未対応の基数です。")


# =========================
# Signed / Unsigned
# =========================

def to_signed_value(unsigned_value: int, bit_width: int) -> int:
    sign_bit = 2 ** (bit_width - 1)

    if unsigned_value >= sign_bit:
        return unsigned_value - (2 ** bit_width)

    return unsigned_value


def to_unsigned_twos_complement(dec_value: int, bit_width: int) -> int:
    min_val = -(2 ** (bit_width - 1))
    max_val = (2 ** (bit_width - 1)) - 1

    if dec_value < min_val or dec_value > max_val:
        raise ValueError(f"{bit_width}bit signed の範囲外です。範囲: {min_val} ～ {max_val}")

    if dec_value < 0:
        return (2 ** bit_width) + dec_value

    return dec_value


def check_unsigned_range(value: int, bit_width: int) -> None:
    max_val = (2 ** bit_width) - 1

    if value < 0 or value > max_val:
        raise ValueError(f"{bit_width}bit unsigned の範囲外です。範囲: 0 ～ {max_val}")


# =========================
# Output formatter
# =========================

def make_outputs(unsigned_value: int, bit_width: int, sep: bool) -> dict:
    bin_s = dec_to_base(unsigned_value, 2)
    oct_s = dec_to_base(unsigned_value, 8)
    hex_s = dec_to_base(unsigned_value, 16)

    bin_s = pad_left(bin_s, required_digits(bit_width, 2))
    oct_s = pad_left(oct_s, required_digits(bit_width, 8))
    hex_s = pad_left(hex_s, required_digits(bit_width, 16))

    if sep:
        bin_s = group_text(bin_s, 4)
        oct_s = group_text(oct_s, 3)
        hex_s = group_text(hex_s, 4)

    return {
        "binary": bin_s,
        "octal": oct_s,
        "hex": hex_s
    }


# =========================
# Routes
# =========================

@app.route("/")
def index():
    return render_template("index.html")


@app.route("/convert", methods=["POST"])
def convert():
    data = request.get_json()

    bin_s = data.get("binary", "").strip()
    oct_s = data.get("octal", "").strip()
    dec_s = data.get("decimal", "").strip()
    hex_s = data.get("hex", "").strip()
    bit_s = data.get("bit", "").strip()

    sep = bool(data.get("separator", False))
    mode = data.get("mode", "unsigned")

    try:
        if not is_bin(bin_s):
            raise ValueError("Binary value は 0 / 1 / カンマのみ入力できます。")

        if not is_oct(oct_s):
            raise ValueError("Octal value は 0 ～ 7 / カンマのみ入力できます。")

        if not is_dec(dec_s):
            raise ValueError("Decimal value は数字、または先頭の - のみ入力できます。")

        if not is_hex(hex_s):
            raise ValueError("Hex value は 0 ～ 9 / A ～ F / カンマのみ入力できます。")

        if not is_digit(bit_s):
            raise ValueError("Bit width は数字のみ入力できます。")

        inputs = [v for v in [bin_s, oct_s, dec_s, hex_s] if v != ""]

        if len(inputs) == 0:
            raise ValueError("BIN / OCT / DEC / HEX のいずれか1つに入力してください。")

        if len(inputs) >= 2:
            return jsonify({
                "ok": False,
                "clear": True,
                "message": "複数の入力欄に値があります。\n全て消去します。"
            })

        if bit_s == "":
            raise ValueError("Bit width を指定してください。")

        bit_width = int(bit_s)

        if bit_width <= 0:
            raise ValueError("Bit width には1以上を指定してください。")

        # -------------------------
        # 入力値を unsigned_value に統一
        # -------------------------

        if bin_s:
            unsigned_value = base_to_dec(bin_s, 2)

        elif oct_s:
            unsigned_value = base_to_dec(oct_s, 8)

        elif hex_s:
            unsigned_value = base_to_dec(hex_s, 16)

        else:
            if dec_s == "-":
                raise ValueError("Decimal value が不正です。")

            dec_value = int(dec_s)

            if mode == "signed":
                unsigned_value = to_unsigned_twos_complement(dec_value, bit_width)
            else:
                if dec_value < 0:
                    raise ValueError("unsignedでは負数を変換できません。signedを選択してください。")

                unsigned_value = dec_value

        # -------------------------
        # bit幅範囲チェック
        # -------------------------

        check_unsigned_range(unsigned_value, bit_width)

        # -------------------------
        # DEC表示
        # -------------------------

        if mode == "signed":
            decimal_value = to_signed_value(unsigned_value, bit_width)
        else:
            decimal_value = unsigned_value

        outputs = make_outputs(unsigned_value, bit_width, sep)

        return jsonify({
            "ok": True,
            "binary": outputs["binary"],
            "octal": outputs["octal"],
            "decimal": str(decimal_value),
            "hex": outputs["hex"],
            "message": "Conversion completed."
        })

    except ValueError as e:
        return jsonify({
            "ok": False,
            "message": str(e)
        })


if __name__ == "__main__":
    app.run(debug=True, port=5000)
