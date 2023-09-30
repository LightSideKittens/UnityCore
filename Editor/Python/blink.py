import sys
import argparse
from PyQt5 import QtCore, QtWidgets
from PyQt5.QtCore import Qt, QPropertyAnimation
from PyQt5.QtWidgets import QApplication, QWidget
import win32gui, win32con


class Rect:
    def __init__(self, x, y, w, h):
        self.x = x
        self.y = y
        self.w = w
        self.h = h


class AnimatedRectangle(QtWidgets.QMainWindow):
    def __init__(self, rect, cycles, color, max_opacity):
        super().__init__()

        self.setGeometry(rect.x, rect.y, rect.w, rect.h)
        self.setWindowOpacity(0.0)  # Start fully transparent

        # Set window transparency and remove title bar
        self.setWindowFlags(Qt.FramelessWindowHint | Qt.WindowStaysOnTopHint | Qt.Tool)

        # Enable the window to have a transparent background
        self.setAttribute(Qt.WA_TranslucentBackground)
        self.setAttribute(Qt.WA_TransparentForMouseEvents, True)

        # Create a QWidget with a specific color
        self.widget = QWidget(self)
        self.widget.setAttribute(Qt.WA_TransparentForMouseEvents, True)
        self.widget.setStyleSheet(f"background-color: {color};")
        self.widget.resize(rect.w, rect.h)

        # Animate the window opacity to appear and disappear
        self.animation = QPropertyAnimation(self, b"windowOpacity")
        self.animation.setDuration(200)  # 1000 ms = 1 second
        self.animation.finished.connect(self.nextCycle)

        self.cycles = cycles  # Number of flash cycles
        self.cycle_count = 0  # Counter for flash cycles
        self.max_opacity = max_opacity  # Maximum opacity
        self.flash()

    def flash(self):
        if self.windowOpacity() == 0.0:
            # If window is transparent, make it opaque
            self.animation.setStartValue(0.0)
            self.animation.setEndValue(self.max_opacity)
        else:
            # If window is opaque, make it transparent
            self.animation.setStartValue(self.max_opacity)
            self.animation.setEndValue(0.0)

        self.animation.start()

    def nextCycle(self):
        self.cycle_count += 1
        if self.cycle_count < self.cycles:
            self.flash()
        else:
            self.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("x", type=int)
    parser.add_argument("y", type=int)
    parser.add_argument("w", type=int)
    parser.add_argument("h", type=int)
    parser.add_argument("cycles", type=int)
    parser.add_argument("color", type=str)
    parser.add_argument("max_opacity", type=float)
    args = parser.parse_args()

    app = QApplication(sys.argv)
    rect = Rect(args.x, args.y, args.w, args.h)  # Rectangle parameters
    cycles = args.cycles  # Number of flash cycles
    color = args.color  # Color
    max_opacity = args.max_opacity  # Maximum opacity
    animated_rect = AnimatedRectangle(rect, cycles, color, max_opacity)
    animated_rect.show()

    hwnd = animated_rect.winId().__int__()  # get the HWND value
    # set the window as a click-through window
    style = win32gui.GetWindowLong(hwnd, win32con.GWL_EXSTYLE)
    style |= win32con.WS_EX_TRANSPARENT | win32con.WS_EX_LAYERED
    win32gui.SetWindowLong(hwnd, win32con.GWL_EXSTYLE, style)

    sys.exit(app.exec_())
