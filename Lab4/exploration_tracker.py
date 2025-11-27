import numba
import numpy as np

from Lab4.env_analizators import get_scanning_points
from Lab4.state import State


@numba.njit
def mark_scanned_line(mask, r0, c0, r1, c1):
    """
    Marks pixels along a scan ray as 'seen'.
    Returns number of newly seen pixels.
    """
    new_pixels = 0
    if r0 == r1: # Horizontal line
        c_start = min(c0, c1)
        c_end = max(c0, c1)
        for c in range(c_start, c_end + 1):
             if 0 <= r0 < mask.shape[0] and 0 <= c < mask.shape[1]:
                if mask[r0, c] == 0:
                    mask[r0, c] = 2
                    new_pixels += 1
    elif c0 == c1: # Vertical line
        r_start = min(r0, r1)
        r_end = max(r0, r1)
        for r in range(r_start, r_end + 1):
            if 0 <= r < mask.shape[0] and 0 <= c0 < mask.shape[1]:
                if mask[r, c0] == 0:
                    mask[r, c0] = 2
                    new_pixels += 1
    return new_pixels

@numba.njit
def mark_scanned_traces_from_box(mask, box, up_wall, down_wall, left_wall, right_wall):
    """
    Marks the scan lines from the player box to the walls.
    Returns number of newly seen pixels.
    """
    left_points, right_points, up_points, down_points = get_scanning_points(box)
    new_pixels = 0

    for point in left_points:
        new_pixels += mark_scanned_line(mask, point[0], point[1], left_wall[0], left_wall[1])
    for point in right_points:
        new_pixels += mark_scanned_line(mask, point[0], point[1], right_wall[0], right_wall[1])
    for point in up_points:
        new_pixels += mark_scanned_line(mask, point[0], point[1], up_wall[0], up_wall[1])
    for point in down_points:
        new_pixels += mark_scanned_line(mask, point[0], point[1], down_wall[0], down_wall[1])

    return new_pixels

@numba.njit
def mark_pixels_in_box(mask, r_min, c_min, r_max, c_max):
    """
    Marks the rectangular area covered by the player.
    Returns the number of *newly* visited pixels.
    """
    r_min = max(0, min(r_min, mask.shape[0]))
    c_min = max(0, min(c_min, mask.shape[1]))
    r_max = max(0, min(r_max, mask.shape[0]))
    c_max = max(0, min(c_max, mask.shape[1]))

    new_pixels = 0
    for r in range(r_min, r_max):
        for c in range(c_min, c_max):
            if mask[r, c] == 0:
                mask[r, c] = 1 # 1 = Visited by Player Body
                new_pixels += 1
    return new_pixels

class ExplorationTracker:
    def __init__(self, h, w):
        self.height = h
        self.width = w
        self.space = np.zeros((w, h), dtype=np.int32)

    def reset(self):
        self.space = np.zeros((self.width, self.height), dtype=np.int32)

    def cover(self, state: State):
        """
        :return: new visited pixels, new scanned pixels
        """

        if state.player_box is None:
            return 0, 0

        r_min, c_min, r_max, c_max = state.player_box
        new_visited = mark_pixels_in_box(self.space, r_min, c_min, r_max, c_max)

        new_scanned = mark_scanned_traces_from_box(self.space, state.player_box, state.up_wall, state.down_wall, state.left_wall, state.right_wall)

        return new_visited, new_scanned