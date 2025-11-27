import numpy as np
from env_analizators import *

VECTOR_STATE_SIZE = 16

class State:
    def __init__(self, frame):
        frame = cut_empty_layers_in_frame(frame)
        state, enemy_pixels = rgb_to_index(frame)
        self.is_empty = len(state) == 0 or len(state[0]) == 0
        if not self.is_empty:
            fill_holes(state)
            self.state = state
            self.state_h = state.shape[0]
            self.state_w = state.shape[1]
            self.player_box = find_player_box(state)
            obs = basic_observation(state, self.player_box) if not self.player_box is None else None

            self.left_wall = obs[0] if obs is not None and obs[0] is not None else (0, 0)
            self.right_wall = obs[1] if obs is not None and obs[1] is not None else (0, 0)
            self.up_wall = obs[2] if obs is not None and obs[2] is not None else (0, 0)
            self.down_wall = obs[3] if obs is not None and obs[3] is not None else (0, 0)
            self.closest_enemy = obs[4] if obs is not None and obs[4] is not None else None
            self.closest_portal = obs[5] if obs is not None and obs[5] is not None else None
            self.enemies = np.round(float(enemy_pixels) / AVG_PIXELS_IN_ENEMY).astype(np.int32)

            self.area = self.state_h * self.state_w
        else:
            self.player_box = None
            self.area = 0

    def has_enemy(self):
        return self.enemies > 0 or self.closest_enemy is not None

    def percentage_from_area(self, pixels):
        if self.is_empty or self.area == 0:
            return 0.0
        return pixels / self.area

    def get_direction_on_closest_wall(self):
        center_of_player = self.center_of_player()
        up_dist = center_of_player[0] - self.up_wall[0]
        down_dist = self.down_wall[0] - center_of_player[0]
        left_dist = center_of_player[1] - self.left_wall[1]
        right_dist = self.right_wall[1] - center_of_player[1]

        dists = [up_dist, right_dist, left_dist, down_dist]
        min_dist = min(dists)
        return dists.index(min_dist)

    def get_distance_to_closest_wall(self):
        center_of_player = self.center_of_player()
        up_dist = center_of_player[0] - self.up_wall[0]
        down_dist = self.down_wall[0] - center_of_player[0]
        left_dist = center_of_player[1] - self.left_wall[1]
        right_dist = self.right_wall[1] - center_of_player[1]

        dists = [up_dist, right_dist, left_dist, down_dist]
        min_dist = min(dists)
        return min_dist

    def as_vector(self):
        player_center = self.center_of_player()
        px_norm = player_center[0] / self.state_h
        py_norm = player_center[1] / self.state_w

        up_wall_dist = player_center[0] - self.up_wall[0]
        down_wall_dist = self.down_wall[0] - player_center[0]
        left_wall_dist = player_center[1] - self.left_wall[1]
        right_wall_dist = self.right_wall[1] - player_center[1]

        dist_up_norm = up_wall_dist / self.state_h
        dist_down_norm = down_wall_dist / self.state_h
        dist_left_norm = left_wall_dist / self.state_w
        dist_right_norm = right_wall_dist / self.state_w

        epsilon = 1e-3
        inv_up = 1.0 / (dist_up_norm + epsilon)
        inv_down = 1.0 / (dist_down_norm + epsilon)
        inv_left = 1.0 / (dist_left_norm + epsilon)
        inv_right = 1.0 / (dist_right_norm + epsilon)

        enemy_x_norm = self.closest_enemy[0] / self.state_h if self.closest_enemy is not None else 0
        enemy_y_norm = self.closest_enemy[1] / self.state_w if self.closest_enemy is not None else 0
        enemy_visible = 1.0 if self.closest_enemy is not None else 0.0

        enemy_count = self.enemies / MAX_ENEMIES
        portal_x_norm = self.closest_portal[0] / self.state_h if self.closest_portal is not None else 0
        portal_y_norm = self.closest_portal[1] / self.state_w if self.closest_portal is not None else 0

        return np.array([px_norm, py_norm,
                         dist_up_norm, dist_down_norm,
                         dist_left_norm, dist_right_norm,
                         inv_up, inv_down,
                         inv_left, inv_right,
                         enemy_x_norm, enemy_y_norm,
                         enemy_visible, enemy_count, portal_x_norm, portal_y_norm], dtype=np.float32)

    def center_of_player(self):
        if self.player_box is None:
            return 0, 0
        i_center = (self.player_box[0] + self.player_box[2]) // 2
        j_center = (self.player_box[1] + self.player_box[3]) // 2
        return i_center, j_center

    def distance_from_player(self, cord, failure_val=-1):
        if self.is_empty or self.player_box is None:
            return failure_val

        player_x, player_y = self.center_of_player()
        return np.sqrt((player_x - cord[0]) ** 2 + (player_y - cord[1]) ** 2)

    def distance_to_closest_border(self):
        player_x, player_y = self.center_of_player()
        up_dist = player_x - self.up_wall[0]
        down_dist = self.down_wall[0] - player_x
        left_dist = player_y - self.left_wall[1]
        right_dist = self.right_wall[1] - player_y
        return min(up_dist, down_dist, left_dist, right_dist)

    def direction_to_player(self, cord):
        i, j = cord
        left_up_corner = (self.player_box[0], self.player_box[1])
        left_down_corner = (self.player_box[2], self.player_box[1])
        right_up_corner = (self.player_box[0], self.player_box[3])
        right_down_corner = (self.player_box[2], self.player_box[3])

        if i < left_up_corner[0]:
            if j < left_up_corner[1]:
                return UPLEFT_DIRECTION
            elif j > right_up_corner[1]:
                return UPRIGHT_DIRECTION
            else:
                return UP_DIRECTION
        elif i > left_down_corner[0]:
            if j < left_down_corner[1]:
                return DOWNLEFT_DIRECTION
            elif j > right_down_corner[1]:
                return DOWNRIGHT_DIRECTION
            else:
                return DOWN_DIRECTION
        else:
            if j < left_up_corner[1]:
                return LEFT_DIRECTION
            elif j > right_up_corner[1]:
                return RIGHT_DIRECTION
            else:
                return -1