import numpy

from convex import ConvexHull, Point


class MyPoint(object):
    def __init__(self, c_x: float, c_y: float):
        self.x: float = c_x
        self.y: float = c_y


def calculate_std(center: MyPoint, list_samples: list):
    result_samples: list = []

    s: float = 0

    for sample in list_samples:
        dx = sample.x - center.x
        dy = sample.y - center.y

        d = dx * dx + dy * dy

        result_samples.append((sample, d))

        s += d

    s /= len(list_samples)

    return result_samples, numpy.sqrt(s)

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
        ##print(f'{self.index}, {len(self.list_samples)}')
        ch = ConvexHull()

        ##list_points: list = ch.compute_hull(self.list_samples)

        self.convex_hull = ch.compute_hull(self.list_samples)

    def calculate_std(self):
        new_centroids: list = []

        if isinstance(self.center, MyPoint) and isinstance(self.list_samples, list) and len(self.list_samples) > 0:
            std_tuple = calculate_std(self.center,  self.list_samples)

            result_samples: Sample = std_tuple[0]
            std: float = std_tuple[1]

            var: float = std * std

            centroid_in: Centroid = Centroid(0, 0, 0)
            centroid_out: Centroid = Centroid(0, 0, 0)

            for tuple_sample in result_samples:
                sample = tuple_sample[0]
                d = tuple_sample[1]

                num_var: float = numpy.abs(d / var)

                if num_var < 3:
                    centroid_in.list_samples.append(sample)
                else:
                    centroid_out.list_samples.append(sample)

            len_in = len(centroid_in.list_samples)
            len_out = len(centroid_out.list_samples)

            ##print(f'len in = {len_in}, len out = {len_out}')

            critical_number: float = 100## len(result_samples) / 20

            if len_in > critical_number and len_out > critical_number:
                for sample in centroid_in.list_samples:
                    sample.centroid = centroid_in

                new_centroids.append(centroid_in)

                for sample in centroid_out.list_samples:
                    sample.centroid = centroid_out

                new_centroids.append(centroid_out)

        return new_centroids



class Sample(MyPoint):
    def __init__(self, c_x: float, c_y: float, cent: Centroid = None):
        super().__init__(c_x, c_y)

        self.centroid = cent
