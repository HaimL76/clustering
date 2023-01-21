class MyPoint:
	def __init__(self, c_x, c_y):
		self.x: float = c_x
		self.y: float = c_y


class Centroid:
	def __init__(self, c_x, c_y):
		self.center: MyPoint = MyPoint(c_x, c_y)
		self.list_samples: list = []

	def calculate_center(self):
		if isinstance(self.list_samples, list) and len(self.list_samples) > 0:
			s_x = 0
			s_y = 0

			for sample in self.list_samples:
				s_x += sample.x
				s_y += sample.y

			len0 = len(self.list_samples)

			self.center = MyPoint(s_x / len0, s_y / len0)