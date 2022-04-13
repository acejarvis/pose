import sys
import cv2
from openni import openni2, nite2, utils
import numpy as np
import argparse

GRAY_COLOR = (64, 64, 64)
CAPTURE_SIZE_REALSENSE = (1024, 768) # Intel Realsense L515 under USB 3.0 connection


def parse_arg():
    parser = argparse.ArgumentParser()
    parser.add_argument('-w', '--window_width', type=int, default=1024)
    return parser.parse_args()

def draw_connection(img, tracker, joint1, joint2, col):
    (x1, y1) = tracker.convert_joint_coordinates_to_depth(
        joint1.position.x, joint1.position.y, joint1.position.z)
    (x2, y2) = tracker.convert_joint_coordinates_to_depth(
        joint2.position.x, joint2.position.y, joint2.position.z)

    if (joint1.positionConfidence > 0.4 and joint2.positionConfidence > 0.4):
        if joint1.positionConfidence < 1.0 or joint2.positionConfidence < 1.0:
            c = GRAY_COLOR
        else:
            c = col
        cv2.line(img, (int(x1), int(y1)), (int(x2), int(y2)), c, 1)

        if joint1.positionConfidence < 1.0:
            c = GRAY_COLOR
        else:
            c = col
        cv2.circle(img, (int(x1), int(y1)), 2, c, -1)

        if joint2.positionConfidence < 1.0:
            c = GRAY_COLOR
        else:
            c = col
        cv2.circle(img, (int(x2), int(y2)), 2, c, -1)

def draw_skeleton(img, tracker, user, col):
    for idx1, idx2 in [
        # upper body
        (nite2.JointType.NITE_JOINT_HEAD,
         nite2.JointType.NITE_JOINT_NECK),
        (nite2.JointType.NITE_JOINT_LEFT_COLLAR,
         nite2.JointType.NITE_JOINT_NECK),
        (nite2.JointType.NITE_JOINT_LEFT_COLLAR,
         nite2.JointType.NITE_JOINT_LEFT_SHOULDER),
        (nite2.JointType.NITE_JOINT_LEFT_COLLAR,
         nite2.JointType.NITE_JOINT_RIGHT_SHOULDER),
        (nite2.JointType.NITE_JOINT_LEFT_COLLAR,
         nite2.JointType.NITE_JOINT_TORSO),

        # left hand
        (nite2.JointType.NITE_JOINT_LEFT_WRIST,
         nite2.JointType.NITE_JOINT_LEFT_ELBOW),
        (nite2.JointType.NITE_JOINT_LEFT_ELBOW,
         nite2.JointType.NITE_JOINT_LEFT_SHOULDER),
        # right hand
        (nite2.JointType.NITE_JOINT_RIGHT_WRIST,
         nite2.JointType.NITE_JOINT_RIGHT_ELBOW),
        (nite2.JointType.NITE_JOINT_RIGHT_ELBOW,
         nite2.JointType.NITE_JOINT_RIGHT_SHOULDER),
        # lower body
        (nite2.JointType.NITE_JOINT_WAIST,
         nite2.JointType.NITE_JOINT_LEFT_HIP),
        (nite2.JointType.NITE_JOINT_WAIST,
         nite2.JointType.NITE_JOINT_RIGHT_HIP),
        (nite2.JointType.NITE_JOINT_TORSO,
         nite2.JointType.NITE_JOINT_WAIST),

        # left leg
        (nite2.JointType.NITE_JOINT_LEFT_FOOT,
         nite2.JointType.NITE_JOINT_LEFT_KNEE),
        (nite2.JointType.NITE_JOINT_LEFT_KNEE,
         nite2.JointType.NITE_JOINT_LEFT_HIP),
        # right leg
        (nite2.JointType.NITE_JOINT_RIGHT_FOOT,
         nite2.JointType.NITE_JOINT_RIGHT_KNEE),
        (nite2.JointType.NITE_JOINT_RIGHT_KNEE,
         nite2.JointType.NITE_JOINT_RIGHT_HIP)]:
        draw_connection(
            img, tracker, user.skeleton.joints[idx1], user.skeleton.joints[idx2], col)


# -------------------------------------------------------------
# main program from here
# -------------------------------------------------------------

def init_capture_device_realsense():

    openni2.initialize()
    nite2.initialize()
    return openni2.Device.open_any()


def close_capture_device():
    nite2.unload()
    openni2.unload()


def track_skeleton():
    args = parse_arg()
    dev = init_capture_device_realsense()

    dev_name = dev.get_device_info().name.decode('UTF-8')
    print("Device Name: {}".format(dev_name))

    try:
        user_tracker = nite2.UserTracker(dev)
    except utils.NiteError:
        print("Unable to start the NiTE human tracker. Exiting.")
        sys.exit(-1)

    (img_w, img_h) = CAPTURE_SIZE_REALSENSE # (args.window_width, args.window_height)
    win_w = args.window_width
    win_h = int(img_h * win_w / img_w)

    while True:
        tracker_frame = user_tracker.read_frame()

        depth_frame = tracker_frame.get_depth_frame()
        depth_frame_data = depth_frame.get_buffer_as_uint16()
        img = np.ndarray((depth_frame.height, depth_frame.width), dtype=np.uint16,
                         buffer=depth_frame_data).astype(np.float32)
        
        img = img[0:img_h, 0:img_w]

        (min_val, max_val, min_loc, max_loc) = cv2.minMaxLoc(img)
        if (min_val < max_val):
            img = (img - min_val) / (max_val - min_val)
        img = cv2.cvtColor(img, cv2.COLOR_GRAY2RGB)

        if tracker_frame.users:
            for user in tracker_frame.users:
                if user.is_new():
                    print("new human id:{} detected.".format(user.id))
                    user_tracker.start_skeleton_tracking(user.id)
                elif (user.state == nite2.UserState.NITE_USER_STATE_VISIBLE and
                      user.skeleton.state == nite2.SkeletonState.NITE_SKELETON_TRACKED):
                    draw_skeleton(img, user_tracker, user, (255, 0, 0))

        cv2.imshow("Depth", cv2.resize(img, (win_w, win_h)))
        if (cv2.waitKey(1) & 0xFF == ord('q')):
            break

    close_capture_device()


if __name__ == '__main__':
    track_skeleton()
