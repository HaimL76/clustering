from collections import namedtuple  
import matplotlib.pyplot as plt  
import random

from linked_list import LinkedList

Point = namedtuple('Point', 'x y')


def get_orientation(origin, p1, p2):
    '''
    Returns the orientation of the Point p1 with regards to Point p2 using origin.
    Negative if p1 is clockwise of p2.
    :param p1:
    :param p2:
    :return: integer
    '''
    difference = (
            ((p2.x - origin.x) * (p1.y - origin.y))
            - ((p1.x - origin.x) * (p2.y - origin.y))
    )

    return difference

class ConvexHull(object):  
    _points = []
    _hull_points = []

    def __init__(self, calculate_diameter: bool = True):
        self.calculate_convex_hull_diameter: bool = calculate_diameter

    def add(self, point):
        self._points.append(point)

    def compute_hull(self, points: LinkedList):
        diagonal_points: tuple = None
        '''
        Computes the points that make up the convex hull.
        :return:
        '''
        ##if not isinstance(points, LinkedList) or not points.any():
        if not isinstance(points, list) or len(points) < 3:
            return None

        max_squared_distance: float = 0

        hull_points: list = []

        # get leftmost point
        start = None# points[0]
        min_x = None# start.x

        for p in points:#[1:]:
            if start is None and min_x is None:
                start = p
                min_x = p.x
            else:
                if p.x < min_x:
                    min_x = p.x
                    start = p

        point = start

        hull_points.append(start)

        far_point = None
        while far_point is not start:

            # get the first point (initial max) to use to compare with others
            p1 = None
            for p in points:
                if p is point:
                    continue
                else:
                    p1 = p
                    break

            far_point = p1

            for p2 in points:
                # ensure we aren't comparing to self or pivot point
                if p2 is point or p2 is p1:
                    continue
                else:
                    direction = get_orientation(point, far_point, p2)
                    if direction > 0:
                        far_point = p2

            if self.calculate_convex_hull_diameter and len(hull_points) > 0:
                for point in hull_points:
                    dx: float = point.x - far_point.x
                    dy: float = point.y - far_point.y

                    d: float = dx * dx + dy * dy

                    if d > max_squared_distance:
                        max_squared_distance = d
                        diagonal_points = (point, far_point)

            hull_points.append(far_point)

            point = far_point

        return hull_points, diagonal_points, max_squared_distance

    def get_hull_points(self):
        if self._points and not self._hull_points:
            self.compute_hull()

        return self._hull_points

    def display(self):
        # all points
        x = [p.x for p in self._points]
        y = [p.y for p in self._points]
        plt.plot(x, y, marker='D', linestyle='None')

        # hull points
        hx = [p.x for p in self._hull_points]
        hy = [p.y for p in self._hull_points]
        plt.plot(hx, hy)

        plt.title('Convex Hull')
        plt.show()


def main():  
    ch = ConvexHull()
    for _ in range(50):
        ch.add(Point(random.randint(-100, 100), random.randint(-100, 100)))

    print("Points on hull:", ch.get_hull_points())
    ch.display()


if __name__ == '__main__':  
    main()