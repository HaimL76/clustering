from convex import ConvexHull, Point


class MyPoint(object):
    def __init__(self, c_x: float, c_y: float):
        self.x: float = c_x
        self.y: float = c_y


class Centroid(object):
    def __init__(self, i: int, c_x: float, c_y: float):
        self.center: MyPoint = MyPoint(c_x, c_y)
        self.list_samples: list = []
        self.index = i
        self.convex_hull = []

    def calculate_center(self):
        if isinstance(self.list_samples, list) and len(self.list_samples) > 0:
            s_x = 0
            s_y = 0

            for sample in self.list_samples:
                s_x += sample.x
                s_y += sample.y

            len0 = len(self.list_samples)

            self.center = MyPoint(s_x / len0, s_y / len0)

    def calculate_convex_hull(self):
        print(f'{self.index}, {len(self.list_samples)}')
        ch = ConvexHull()

        for sample in self.list_samples:
            ch.add(Point(sample.x, sample.y))

        list_points: list = ch.compute_hull(self.list_samples)

        if isinstance(list_points, list) and len(list_points) > 0:
            self.convex_hull = list(map(lambda p: MyPoint(p.x, p.y), list_points))


class Sample(MyPoint):
    def __init__(self, c_x: float, c_y: float, cent: Centroid=None):
        super().__init__(c_x, c_y)

        self.centroid = cent
