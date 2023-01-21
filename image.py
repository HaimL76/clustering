import cv2
import numpy as np

from mypoint import Sample


def cluster_image(full_path: str):
    print("hello, world!")
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

    ##print(type(ret))
    ##print(type(dst))

    num_rows = corners.shape[0]
    num_cols = corners.shape[1]

    print(f'num rows = {num_rows}')
    print(f'num cols = {num_cols}')

    thresh = 0.01 * corners.max()

    list_samples: list = []

    for i in range(num_rows):
        for j in range(num_cols):
            if (corners[i, j] > thresh):
                list_samples.append(Sample(i, j))
                ##image[i, j] = [255, 0, 0]



    ##print(f'list corners = {list_corners}')

    # Threshold for an optimal value.
    ##image[corners > 0.01 * corners.max()] = [0, 0, 255]

    ##print(f'corners = {corners}, {type(corners)}')

    # the window showing output image with corners
    cv2.imshow('Image with Corners', image)
    cv2.waitKey(0)
    cv2.destroyAllWindows()
