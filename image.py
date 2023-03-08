import cv2
import numpy as np

import pandas as pd
from matplotlib import pyplot as plt
#from sklearn.datasets.samples_generator import make_blobs
from sklearn.cluster import KMeans

from kmeans_implemented import KMeansImplemented
from mypoint import Sample, Centroid


def get_corners(full_path: str) -> list:
    print(full_path)
    import multiprocessing as mp
    print("Number of processors: ", mp.cpu_count())

    # Reading an image in default mode
    image = cv2.imread(full_path)

    # convert the input image into grayscale
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # modify the data type setting to 32-bit floating point
    gray = np.float32(gray)

    # apply the cv2.cornerHarris method to detect the corners
    corners = cv2.cornerHarris(gray, 2, 3, 0.05)

    # result is dilated for marking the corners
    ##corners = cv2.dilate(corners, None)

    ret, dst = cv2.threshold(corners, 0.01 * corners.max(), 255, 0)

    num_rows = corners.shape[0]
    num_cols = corners.shape[1]

    print(f'num rows = {num_rows}')
    print(f'num cols = {num_cols}')

    thresh = 0.01 * corners.max()

    list_samples: list = []

    for i in range(num_rows):
        for j in range(num_cols):
            if corners[i, j] > thresh:
                list_samples.append(Sample(j, i))

    return image, list_samples


def display_clusters(image, centroids: list):
    if not isinstance(centroids, list):
        raise ValueError("centroids")

    if len(centroids) < 1:
        raise ValueError("centroids")

    print(len(centroids))

    colors = 255 * 255 * 255

    quant_colors = colors / len(centroids)

    size = 4

    half_size = size / 2

    for i in range(len(centroids)):
        centroid = centroids[i]

        color: int = i * quant_colors

        r: int = color % 255
        g: int = (color / 255) % 255
        b: int = (color / 255) / 255

        centroid.calculate_convex_hull()

        # Green color in BGR
        color = (r, g, b)
        color0 = (b, g, r)

        # Line thickness of 9 px
        thickness = 3

        prev_point = None

        if centroid.convex_hull:
            # print(f'{centroid.index}, {centroid.list_samples.get_count()}, {len(centroid.convex_hull)}, {centroid.center.x}, {centroid.center.y}')
            print(
                f'{centroid.index}, {len(centroid.list_samples)}, {len(centroid.convex_hull)}, {centroid.center.x}, {centroid.center.y}')

        center_point = (int(centroid.center.x), int(centroid.center.y))

        cv2.putText(img=image, text=f'{centroid.index}',
                    org=(center_point[0], center_point[1]), fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
                    color=color, thickness=1)

        ##print(f'center point = {center_point}')

        ##print(f'{curr_point.x}, {curr_point.y}, {num_cols}, {num_rows}')

        #####if prev_point is not None:
        #######cv2.line(image, (prev_point.y, prev_point.x), (curr_point.y, curr_point.x), color=color, thickness=thickness)

        ##prev_point = curr_point

        font = cv2.FONT_HERSHEY_SIMPLEX

        ##cv2.putText(img=image, text=f'{centroid.index}, {center_point[0]}, {center_point[1]}', org=center_point, fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
        ##          color=(0, 0, 255), thickness=1)

        for sample in centroid.list_samples:
            y = int(sample.y)
            x = int(sample.x)

            if False:  # y < num_rows and x < num_cols:
                cv2.putText(img=image, text=f'{centroid.index}',
                            org=(x, y), fontFace=cv2.FONT_HERSHEY_TRIPLEX, fontScale=1,
                            color=color, thickness=1)

        if isinstance(centroid.convex_hull, list) and len(centroid.convex_hull) > 2:
            for curr_point in centroid.convex_hull:
                if curr_point and prev_point:
                    cv2.line(image, (prev_point.x, prev_point.y), (curr_point.x, curr_point.y), color=color,
                             thickness=1)

                prev_point = curr_point

        if isinstance(centroid.diagonal_points, tuple) and len(centroid.diagonal_points) == 2:
            first_point: MyPoint = centroid.diagonal_points[0]
            second_point: MyPoint = centroid.diagonal_points[1]

            if curr_point and prev_point:
                cv2.line(image, (first_point.x, first_point.y), (second_point.x, second_point.y), color=color0,
                         thickness=1)

    ##print(f'list corners = {list_corners}')

    # Threshold for an optimal value.
    ##image[corners > 0.01 * corners.max()] = [0, 0, 255]

    ##print(f'corners = {corners}, {type(corners)}')

    # the window showing output image with corners
    cv2.imshow('Image with Corners', image)
    cv2.waitKey(0)
    cv2.destroyAllWindows()


def cluster_image_with_lib(full_path: str, k_max: int, k_iteration_index_quant: int = 0, display_optimal_k: bool = False):
    if k_iteration_index_quant < 1:
        k_iteration_index_quant = k_max

    tup: tuple = get_corners(full_path)

    image = tup[0]
    list_samples: list = tup[1]

    if isinstance(list_samples, list) and len(list_samples) > 0:
        list0: list = list(map(lambda sample: [sample.x, sample.y], list_samples))

        X = np.array(list0)

        centroids: list = []

        prev_inertia_ = None

        k_start: int = 3

        max_distances: list = []

        max_num_of_clusters: float = k_max - k_start

        arr: list = [None for i in range(max_num_of_clusters)]

        j: int = 0

        optim_k: int = None
        optim_index_of_k: int = None

        for index_of_k in range(max_num_of_clusters):
            j += 1

            k = k_start + index_of_k

            print(f'k: {k}, index of k: {index_of_k}')

            kmeans = KMeans(n_clusters=k, init='k-means++', max_iter=300, n_init=10, random_state=0)

            kmeans.fit(X)

            inertia: float = kmeans.inertia_

            arr[index_of_k] = (k, inertia, None)

            if j >= k_iteration_index_quant:
                j = 0

                tup_start: tuple = arr[0]
                tup_end: tuple = arr[index_of_k]

                x_start: float = tup_start[0]
                x_end: float = tup_end[0]

                y_start = tup_start[1]
                y_end = tup_end[1]



                calculate_k_with_graph_rotation(x_start, y_start, x_end, y_end, arr, max_num_of_clusters)


                if x_end > x_start:
                    # This is the slope of the line between the first and last points
                    a: float = (y_end - y_start) / (x_end - x_start)

                    # This is the "b" of the line formula
                    b: float = y_start - a * x_start

                    # This is the perpendicular slope (of the perpendicular line, which goes through the curve)
                    p_a: float = 1
                    p_a /= a
                    p_a *= -1

                    max_squared_distance: float = None
                    optim_k = None
                    optim_index_of_k = None

                    for l in range(index_of_k):
                        tup: tuple = arr[l]

                        k = tup[0]

                        # This is the point on the curve
                        x1: float = k
                        y1: float = tup[1]

                        plt.text(x1, y1, str(k))

                        # This is the "b" of the perpendicular line
                        p_b: float = y1 - p_a * x1

                        # Compare the y's: y2 = p_a * x2 + p_b = a * x2 + b
                        # This gives: x2 (p_a - a) = b - p_b
                        # This gives: x2 = (b - p_b) / (pa - a)
                        x2: float = (p_b - b) / (a - p_a)
                        y2: float = p_a * x2 + p_b

                        dx: float = x2 - x1
                        dy: float = y2 - y1

                        d: float = dx * dx + dy * dy

                        if max_squared_distance is None or max_squared_distance < d:
                            max_squared_distance = d
                            optim_k = k
                            optim_index_of_k = l

                    ratio: float = optim_index_of_k / index_of_k

                    print(f'optim index of k: {optim_index_of_k}, wcss: {y_end}, ratio: {ratio}')

                    max_distances.append((x_end, max_squared_distance, optim_k))

        for max_distance in max_distances:
            print(f'max k: {max_distance[0]}, max distance: {max_distance[1]}, optim k: {max_distance[2]}')

        if display_optimal_k and isinstance(optim_k, int):
            # Calculate again, for the optimal k
            kmeans = KMeans(n_clusters=optim_k, init='k-means++', max_iter=300, n_init=10, random_state=0)
            kmeans.fit(X)

        num_clusters: int = kmeans.n_clusters

        print(f'num clusters: {num_clusters}')

        centroids = [None for j in range(num_clusters)]

        labels: list = list(kmeans.labels_)

        centers: list = list(kmeans.cluster_centers_)

        wcss: float = 0

        for index in range(len(labels)):
            sample: Sample = list_samples[index]

            label: int = labels[index]

            centroid: Centroid = centroids[label]

            if centroid is None:
                center = centers[label]
                centroids[label] = Centroid(label, center[0], center[1])
                centroid: Centroid = centroids[label]

            centroid.append_sample(sample)

            dx: float = sample.x - centroid.center.x
            dy: float = sample.y - centroid.center.y

            d: float = dx * dx + dy * dy

            wcss += d

        print(f'wcss: {wcss}')

        plt.plot(list(map(lambda tup: tup[0], arr)), list(map(lambda tup: tup[1], arr)))
        plt.title('Elbow Method')
        plt.xlabel('Number of clusters')
        plt.ylabel('WCSS')
        #plt.plot([x_start, x_end], [y_start, y_end])

        plt.show()

        display_clusters(image, centroids)


def calculate_k_with_graph_rotation(x1, y1, x2, y2, arr_k_wcss, max_num_of_clusters):
    points_totated = rotate_graph(x1, y1, x2, y2, arr_k_wcss, max_num_of_clusters)

    min_tuple = min(points_totated, key=lambda x: x[1])
    print("the k after the rotation is: ", min_tuple[2])

def calculate_alpha(x1, y1, x2, y2):
    dx = x2 - x1
    dy = y2 - y1

    alpha = np.arctan(dy/dx)

    return alpha


def move_point1_to_center(x1, y1, arr_k_wcss, max_num_of_clusters):
    new_arr_k_wcss = [(k_wcss[0] - x1, k_wcss[1] - y1, k_wcss[0]) for k_wcss in arr_k_wcss]
    return new_arr_k_wcss
    

def rotate_points(arr_k_wcss,alpha):
    return [rotate_by_alpha(point[0],point[1],point[2], alpha) for point in arr_k_wcss]


def rotate_by_alpha(x, y, k, alpha):

    new_x = x*np.cos(alpha) - y*np.sin(alpha)
    new_y = x*np.sin(alpha) + y*np.cos(alpha)
    return (new_x, new_y, k)

def rotate_graph(x1, y1,x2,y2, arr_k_wcss, max_num_of_clusters):
    alpha = calculate_alpha(x1, y1,x2,y2)
    points_centered = move_point1_to_center(x1, y1, arr_k_wcss, max_num_of_clusters)
    return rotate_points(points_centered,alpha)



def cluster_image_implemented(full_path: str, k: int, num_rows: int=0, num_cols: int=0):
    tup: tuple = get_corners(full_path)

    image = tup[0]
    list_samples: list = tup[1]

    if isinstance(list_samples, list) and len(list_samples) > 0:
        kmeans: KMeansImplemented = KMeansImplemented()

        centroids = kmeans.get_centroids(list_samples, k, num_rows=num_rows, num_cols=num_cols)

        if isinstance(centroids, list) and len(centroids) > 0:
            index: int = 0

            for centroid in centroids:
                centroid.index = index
                index += 1

            display_clusters(image, centroids)