import numpy
from matplotlib import pyplot as plt

from convex import ConvexHull, Point


class MyPoint(object):
    def __init__(self, c_x: float, c_y: float):
        self.x: float = c_x
        self.y: float = c_y


class Centroid(object):
    def __init__(self, i: int, c_x: float, c_y: float):
        self.center: MyPoint = MyPoint(c_x, c_y)
        self.list_samples: list = []
        self.index: int = i
        self.convex_hull = []
        self.L: float = 0
        self.T: float = 0
        self.R: float = 0
        self.B: float = 0
        self.intercluster_squared_distance: float = 0

    def calculate_standard_deviation(self):
        if isinstance(self.list_samples, list) and len(self.list_samples) > 0:
            s: float = 0

            for sample in self.list_samples:
                dx = sample.x - self.center.x
                dy = sample.y - self.center.y

                d = dx * dx + dy * dy
                sample.squared_distance_from_centroid = d

                s += d

            s /= len(self.list_samples)

            return numpy.sqrt(s)

    def append_sample(self, point: MyPoint):
        if False and isinstance(self.list_samples, list) and len(self.list_samples) > 0:
            for sample in self.list_samples:
                dx: float = sample.x - point.x
                dy: float = sample.y - point.y

                d = dx * dx + dy * dy

                #print(f'd = {d}')

                if d > self.max_distance:
                    self.max_distance = d

        self.list_samples.append(point)

        if point.x < self.L:
            self.L = point.x

        if point.y < self.T:
            self.T = point.y

        if point.x > self.R:
            self.R = point.x

        if point.y > self.B:
            self.B = point.y

    def calculate_center(self):
        if isinstance(self.list_samples, list) and len(self.list_samples) > 0:
            s_x = 0
            s_y = 0

            for sample in self.list_samples:
                s_x += sample.x
                s_y += sample.y

            len0 = len(self.list_samples)

            self.center = MyPoint(s_x / len0, s_y / len0)

    def calculate_convex_hull(self, calculate_center: bool = False, calculate_diameter: bool = True):
        ch = ConvexHull(calculate_diameter=calculate_diameter)

        tup: tuple = ch.compute_hull(self.list_samples)

        if isinstance(tup, tuple) and len(tup) > 0:
            self.convex_hull = tup[0]
            self.intercluster_squared_distance = tup[1]

        print(f'cluster index = {self.index}, cluster intercluster squared distance = {self.intercluster_squared_distance}')

        if isinstance(self.convex_hull, list) and len(self.convex_hull) > 0:
            s_x: float = 0
            s_y: float = 0

            len0: int = len(self.convex_hull)

            if calculate_center:
                for point in self.convex_hull:
                    s_x += point.x
                    s_y += point.y

                self.center = MyPoint(s_x / len0, s_y / len0)

    def get_new_centroids_by_standard_deviation(self):
        new_centroids: list = []

        if isinstance(self.center, MyPoint) and isinstance(self.list_samples, list) and len(self.list_samples) > 0:
            std: float = self.calculate_standard_deviation()

            if std < 0.001:
                return new_centroids

            var: float = std * std

            centroid_in: Centroid = Centroid(0, 0, 0)
            centroid_out: Centroid = Centroid(0, 0, 0)

            stds: dict = {}

            for sample in self.list_samples:
                d = sample.squared_distance_from_centroid

                num_var: float = float(d) / float(var)

                key = int(num_var)

                if key not in stds:
                    stds[key] = 0

                stds[key] += 1

                if num_var < 3:
                    centroid_in.append_sample(sample)
                else:
                    centroid_out.append_sample(sample)

            len_in = len(centroid_in.list_samples)
            len_out = len(centroid_out.list_samples)

            if len_out < 1:
                return new_centroids

            critical_number: float = 38  ## len(result_samples) * 0.26  ##38##35# 100  ## len(result_samples) / 20

            part: float = float(len_out) / float(len_in)

            if True:  # len_in > critical_number and len_out > critical_number:
                centroid_in.center = self.center
                ##if part > 0.05:
                for sample in centroid_in.list_samples:
                    sample.centroid = centroid_in

                new_centroids.append(centroid_in)

            if len_out > critical_number:
                centroid_out.calculate_center()

                for sample in centroid_out.list_samples:
                    sample.centroid = centroid_out

                new_centroids.append(centroid_out)
            else:
                for sample in centroid_out.list_samples:
                    sample.disabled = True

        return new_centroids


class Sample(MyPoint):
    def __init__(self, c_x: float, c_y: float, cent: Centroid = None):
        super().__init__(c_x, c_y)

        self.centroid = cent
        self.squared_distance_from_centroid = None
        self.disabled = False
