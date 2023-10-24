import numpy as np
import cv2

class M1():
	state = 0

	lower_cyan = np.array([40, 50, 0], int)
	upper_cyan = np.array([100, 255, 255], int)

	def FindRed(self, front_img, bottom_img):
		if self.state == 1:
			res = self.FindRedFromImage(bottom_img)
			if res is not None:
				cx = int(res['m10'] / res['m00'])
				cy = int(res['m01'] / res['m00'])
				if abs(int(bottom_img.shape[0] // 2  - cy)) > 10 or abs(int(bottom_img.shape[1] // 2  - cx)) > 10:
					return int(bottom_img.shape[0] // 2  - cy) * 5, int(bottom_img.shape[1] // 2  - cx) * 5, 500, 0
				self.state = 2

		if self.state == 2:
			return 0, 0, -1000, 0

		res = self.FindRedFromImage(front_img)
		if res is None:
			if self.state == 1:
				return 1000, 0, 500, 0
			return None
		cx = int(res['m10'] / res['m00'])
		cy = int(res['m01'] / res['m00'])
		# Secilen c'nin momentinden merkezini hesapla
		self.state = 1
		return 1000, 0, 500, int(front_img.shape[1] // 2  - cx) * 10

	def FindRedFromImage(self, image):
		frame = cv2.resize(image, (200, 150))
		frame = cv2.bitwise_not(frame)
		frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
		frame = cv2.inRange(frame, self.lower_cyan, self.upper_cyan)

		contours, _ = cv2.findContours(
			frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

		if len(contours) == 0:
			return None

		anlik_max_c_alani, secilen_c, scm = 0.0, None, None

		for c in contours:
			M = cv2.moments(c)

			if M['m00'] > anlik_max_c_alani:
				secilen_c , scm , anlik_max_c_alani = c , M , M['m00']
				# Bulunan en buyuk c'yi anlik olarak kaydet
				# Not : scm -> secilen c momenti

		if scm is None:
			return None
		return scm