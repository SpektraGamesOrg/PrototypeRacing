#!/usr/bin/env python3
"""Generates a small, clean, tintable UI sprite set for the Main Menu.
White silhouettes on transparent backgrounds (so they can be tinted in Unity),
except the coin which is pre-colored gold. Everything is supersampled for AA.
"""
import os, math
from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "_Game", "Art", "UI", "Generated")
OUT = os.path.normpath(OUT)
os.makedirs(OUT, exist_ok=True)

SS = 4  # supersample factor
WHITE = (255, 255, 255, 255)


def canvas(size):
    return Image.new("RGBA", (size * SS, size * SS), (0, 0, 0, 0))


def save(img, name, size):
    img = img.resize((size, size), Image.LANCZOS)
    img.save(os.path.join(OUT, name + ".png"))
    print("wrote", name, size, "x", size)


def s(v):
    return int(round(v * SS))


# ---------------------------------------------------------------- rounded rect
def rounded_rect(size, radius, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([s(1), s(1), s(size - 1), s(size - 1)], radius=s(radius), fill=WHITE)
    save(img, name, size)


# ---------------------------------------------------------------- circle
def circle(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    d.ellipse([s(2), s(2), s(size - 2), s(size - 2)], fill=WHITE)
    save(img, name, size)


# ---------------------------------------------------------------- chevron
def chevron(size, name, point_left=True):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    w = s(18)
    cx_far = 78 if point_left else 50
    cx_near = 46 if point_left else 82
    pts = [(s(cx_far), s(24)), (s(cx_near), s(64)), (s(cx_far), s(104))]
    d.line(pts, fill=WHITE, width=w, joint="curve")
    r = w / 2
    for (x, y) in pts:
        d.ellipse([x - r, y - r, x + r, y + r], fill=WHITE)
    save(img, name, size)


# ---------------------------------------------------------------- play triangle
def play(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    pts = [(s(44), s(30)), (s(44), s(98)), (s(102), s(64))]
    # rounded-ish triangle via polygon + small circles at corners
    d.polygon(pts, fill=WHITE)
    r = s(6)
    for (x, y) in pts:
        d.ellipse([x - r, y - r, x + r, y + r], fill=WHITE)
    save(img, name, size)


# ---------------------------------------------------------------- gear
def gear(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    cx, cy = s(64), s(64)
    teeth = 8
    r_out = s(54)
    r_in = s(40)
    tooth_half = math.radians(13)
    for i in range(teeth):
        a = math.radians(i * 360.0 / teeth)
        poly = []
        for (rr, sign) in [(r_in, -1), (r_out, -1), (r_out, 1), (r_in, 1)]:
            ang = a + sign * tooth_half
            poly.append((cx + rr * math.cos(ang), cy + rr * math.sin(ang)))
        d.polygon(poly, fill=WHITE)
    d.ellipse([cx - r_in, cy - r_in, cx + r_in, cy + r_in], fill=WHITE)
    # center hole (transparent)
    hole = s(17)
    d.ellipse([cx - hole, cy - hole, cx + hole, cy + hole], fill=(0, 0, 0, 0))
    save(img, name, size)


# ---------------------------------------------------------------- coin (gold)
def coin(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    cx, cy = s(64), s(64)
    gold = (255, 196, 60, 255)
    gold_dark = (214, 138, 22, 255)
    gold_light = (255, 224, 140, 255)
    d.ellipse([cx - s(56), cy - s(56), cx + s(56), cy + s(56)], fill=gold_dark)
    d.ellipse([cx - s(50), cy - s(50), cx + s(50), cy + s(50)], fill=gold)
    d.ellipse([cx - s(38), cy - s(38), cx + s(38), cy + s(38)], outline=gold_dark, width=s(4))
    # 4-point sparkle
    L, Wp = s(26), s(7)
    star = [
        (cx, cy - L), (cx + Wp, cy - Wp), (cx + L, cy), (cx + Wp, cy + Wp),
        (cx, cy + L), (cx - Wp, cy + Wp), (cx - L, cy), (cx - Wp, cy - Wp),
    ]
    d.polygon(star, fill=gold_light)
    save(img, name, size)


# ---------------------------------------------------------------- cart (shop)
def cart(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    w = s(7)

    def line(pts):
        d.line([(s(x), s(y)) for (x, y) in pts], fill=WHITE, width=w, joint="curve")
        r = w / 2
        for (x, y) in pts:
            d.ellipse([s(x) - r, s(y) - r, s(x) + r, s(y) + r], fill=WHITE)

    # handle + basket outline (trapezoid)
    line([(16, 26), (32, 26), (44, 40), (104, 40), (94, 78), (56, 78), (44, 40)])
    # wheels
    for wx in (58, 90):
        d.ellipse([s(wx) - s(7), s(92) - s(7), s(wx) + s(7), s(92) + s(7)], fill=WHITE)
    save(img, name, size)


# ---------------------------------------------------------------- brush (customize)
def brush(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    # wooden handle (rounded thick diagonal line, top-right -> middle)
    w = s(14)
    pts = [(s(102), s(26)), (s(64), s(64))]
    d.line(pts, fill=WHITE, width=w, joint="curve")
    r = w / 2
    for (x, y) in pts:
        d.ellipse([x - r, y - r, x + r, y + r], fill=WHITE)
    # ferrule (metal band)
    d.polygon([(s(54), s(58), ), (s(72), s(40)), (s(82), s(50)), (s(64), s(68))], fill=WHITE)
    # bristle tip (triangle fanning down-left)
    d.polygon([(s(58), s(56)), (s(74), s(72)), (s(34), s(104))], fill=WHITE)
    save(img, name, size)


# ---------------------------------------------------------------- lock
def lock(size, name):
    img = canvas(size)
    d = ImageDraw.Draw(img)
    cx = s(64)
    # shackle
    d.arc([cx - s(24), s(24), cx + s(24), s(72)], start=180, end=360, fill=WHITE, width=s(10))
    # body
    d.rounded_rectangle([cx - s(34), s(54), cx + s(34), s(104)], radius=s(12), fill=WHITE)
    # keyhole
    d.ellipse([cx - s(8), s(70), cx + s(8), s(86)], fill=(0, 0, 0, 0))
    d.polygon([(cx - s(4), s(80)), (cx + s(4), s(80)), (cx + s(6), s(96)), (cx - s(6), s(96))], fill=(0, 0, 0, 0))
    save(img, name, size)


rounded_rect(96, 30, "ui_round_rect")
circle(160, "ui_circle")
chevron(128, "icon_chevron_left", point_left=True)
chevron(128, "icon_chevron_right", point_left=False)
play(128, "icon_play")
gear(128, "icon_settings")
coin(128, "icon_coin")
cart(128, "icon_cart")
brush(128, "icon_brush")
lock(128, "icon_lock")
print("DONE ->", OUT)
