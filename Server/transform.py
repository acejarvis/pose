import numpy as np
import matplotlib.pyplot as plt
from pytransform3d import rotations as pr
from pytransform3d import transformations as pt
from pytransform3d.transform_manager import TransformManager


def transform(path):

    # file read json
    with open(path) as json_file:
        data = json.load(json_file)

    # transform
    tm = TransformManager()
    tm.load_from_json(data)
    tm.print_transforms()

    cam2wall = pt.transform_from_pq(
        np.hstack((np.array([0.0, 0.0, 0.8]), pr.q_id)))
    object2cam = pt.transform_from(
        pr.active_matrix_from_intrinsic_euler_xyz(np.array([0.0, 0.0, -0.5])),
        np.array([0.5, 0.1, 0.1]))

    tm = TransformManager()
    tm.add_transform("camera", "wall", cam2wall)
    tm.add_transform("object", "camera", object2cam)

    device = tm.get_transform("wall", "object")

    ax = tm.plot_frames_in("wall", s=0.1)
    ax.set_xlim((-2, 2))
    ax.set_ylim((-1, 1))
    ax.set_zlim((-0.5, 2.0))
    plt.show()


def plot3d():
    ax = plt.axes(projection='3d')
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')
    ax.set_xlim(-5, 5)
    ax.set_ylim(0, 5)
    ax.set_zlim(-5, 5)

    #                 left bottom   left top           right top
    plane = np.array([[-1, 0, -2], [-1.2, 3, -2.1], [3, 0.1, -1.5]])

    a_vector = plane[1] - plane[0]
    b_vector = plane[2] - plane[0]

    normal_vector = np.cross(a_vector, b_vector)
    print(normal_vector)
    a_vector = np.concatenate((plane[0], a_vector), axis=0)
    b_vector = np.concatenate((plane[0], b_vector), axis=0)
    normal_vector = np.concatenate((plane[0], normal_vector), axis=0)
    soa = [a_vector, b_vector, normal_vector]

    x, y, z, u ,v, w = zip(*soa)
    ax.quiver(x, y, z, u, v, w, length=1, normalize=True)
    plt.show()


