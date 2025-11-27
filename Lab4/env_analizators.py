import numba
import numpy as np
from Lab4.env_info import *


@numba.njit
def _int_linspace(start, stop, count):
    arr = np.empty(count, dtype=np.int32)
    if count == 1:
        arr[0] = start
        return arr
    step = (stop - start) / (count - 1)
    for k in range(count):
        arr[k] = int(start + k * step)
    return arr

@numba.njit
def find_player_box(state):
    player_positions = np.argwhere(state == PLAYER_INDEX)

    if player_positions.shape[0] == 0:
        return None

    i_pos = player_positions[:, 0]
    j_pos = player_positions[:, 1]
    box = (np.min(i_pos), np.min(j_pos), np.max(i_pos), np.max(j_pos))
    return box

@numba.njit
def get_scanning_points(box):
    """
    :return: left_search_points, right_search_points, up_search_points, down_search_points
    """
    vertical_aligments_search_points_y = _int_linspace(box[0], box[2], 4)
    left_search_points = [(y, box[1]) for y in vertical_aligments_search_points_y]
    right_search_points = [(y, box[3]) for y in vertical_aligments_search_points_y]

    horizontal_search_points_x = _int_linspace(box[1], box[3], 2)
    up_search_points = [(box[0], x) for x in horizontal_search_points_x]
    down_search_points = [(box[2], x) for x in horizontal_search_points_x]

    return left_search_points, right_search_points, up_search_points, down_search_points

@numba.njit
def basic_observation(state, box):
    if box is None:
        return None, None, None, None, None, None

    left_search_points, right_search_points, up_search_points, down_search_points = get_scanning_points(box)

    l_wall = (left_search_points[0][0], 0)
    r_wall = (right_search_points[0][0], state.shape[1] - 1)
    u_wall = (0, up_search_points[0][1])
    d_wall = (state.shape[0] - 1, down_search_points[0][1])

    reach_l, reach_r, reach_u, reach_d = False, False, False, False

    closest_enemy = None
    closest_portal = None

    i = 0
    while True:
        if i > max(state.shape):
            raise ValueError("No walls found in any direction")

        if not reach_l:
            cords = [(point[0], point[1] - i) for point in left_search_points]
            for cord in cords:
                material = state[cord]
                if material == WALL_INDEX or cords[0][1] == 0:
                    reach_l = True
                    l_wall = cord
                if material == PORTAL_INDEX:
                    reach_l = True
                    if closest_portal is None:
                        closest_portal = cord
                elif material == ENEMY_INDEX:
                    if closest_enemy is None:
                        closest_enemy = cord

                if reach_l:
                    break

        if not reach_r:
            cords = [(point[0], point[1] + i) for point in right_search_points]
            for cord in cords:
                material = state[cord]
                if material == WALL_INDEX or cord[1] == state.shape[1] - 1:
                    reach_r = True
                    r_wall = cord
                elif material == PORTAL_INDEX:
                    reach_r = True
                    if closest_portal is None:
                        closest_portal = cord
                elif material == ENEMY_INDEX:
                    if closest_enemy is None:
                        closest_enemy = cord

                if reach_r:
                    break

        if not reach_u:
            cords = [(point[0] - i, point[1]) for point in up_search_points]
            for cord in cords:
                material = state[cord]
                if material == WALL_INDEX or cord[0] == 0:
                    reach_u = True
                    u_wall = cord
                elif material == PORTAL_INDEX:
                    reach_u = True
                    if closest_portal is None:
                        closest_portal = cord
                elif material == ENEMY_INDEX:
                    if closest_enemy is None:
                        closest_enemy = cord

                if reach_u:
                    break

        if not reach_d:
            cords = [(point[0] + i, point[1]) for point in down_search_points]
            for cord in cords:
                material = state[cord]
                if material == WALL_INDEX or cord[0] == state.shape[0] - 1:
                    reach_d = True
                    d_wall = cord
                if material == PORTAL_INDEX:
                    reach_d = True
                    if closest_portal is None:
                        closest_portal = cord
                elif material == ENEMY_INDEX:
                    if closest_enemy is None:
                        closest_enemy = cord

                if reach_d:
                    break

        if reach_l and reach_r and reach_u and reach_d:
            break
        i += 1

    return l_wall, r_wall, u_wall, d_wall, closest_enemy, closest_portal

@numba.njit
def cut_empty_layers_in_frame(frame):
    skip_layers = 0
    while True:
        if np.array_equal(frame[skip_layers][skip_layers], EMPTY_COLOR):
            skip_layers += 1
        else:
            break
    frame = frame[skip_layers:-skip_layers, skip_layers:-skip_layers]

    skip_layers_from_bottom = 0
    while True:
        if np.array_equal(frame[-(skip_layers_from_bottom + 1)][-(skip_layers_from_bottom + 1)], EMPTY_COLOR):
            skip_layers_from_bottom += 1
        else:
            break
    frame = frame[:-skip_layers_from_bottom, :]
    return frame