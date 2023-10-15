import cv2
import numpy as np

# Aracı sadece saga sola cevirecegiz o yuzden yukseklige gerek yok

lower_cyan = np.array([40, 50, 0], int)
upper_cyan = np.array([100, 255, 255], int)
# Renk aralıgı

def TurnToRed(cap) -> int:
	"""
	Kamerada görülen en büyük kırmızı cismi bulup
	araca ne kadar dönmesi gerektiğini söyle
	"""
	dispframe = cap
	window_x_half = cap.shape[0] / 2
	# Kameradan goruntu al
	# frame = cv2.resize(dispframe,(200,150))
	# kucult
	frame = cv2.bitwise_not(dispframe)
	# Renkleri tersine cevir
	frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
	# HSV'ye donustur
	frame = cv2.inRange(frame, lower_cyan, upper_cyan)
	# inRange ile camgobegi rengini bul
	cv2.imshow('inrange',frame)
	# frame = cv2.threshold(frame, 40, 200, cv2.THRESH_BINARY + cv2.THRESH_OTSU)[1]
	# cv2.imshow('threshold',frame)


	# cv2.imshow('cam',dispframe)
	# cv2.waitKey(24)

	contours, _ = cv2.findContours(
		frame, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
	# contour listesini al

	if len(contours) == 0:
		return None
		# c bulamadık o yüzden None döndür

	anlik_max_c_alani, secilen_c, scm = 0.0, None, None

	for c in contours:
		M = cv2.moments(c)
		# Moment hesabından c'nin alanını bul.

		# Anlik karedeki piksel alani en buyuk c'yi bul
		if M['m00'] > anlik_max_c_alani:
			secilen_c , scm , anlik_max_c_alani = c , M , M['m00']
			# Bulunan en buyuk c'yi anlik olarak kaydet
			# Not : scm -> secilen c momenti

	if anlik_max_c_alani < 10:
		return None

	cx = int(scm['m10'] / anlik_max_c_alani)
	# Secilen c'nin momentinden merkezini hesapla
	cv2.drawContours(dispframe,[secilen_c],-1, 0xFF0, cv2.FILLED)
	# cv2.circle(dispframe,(cx,50),1,0xFFF,3)

	cv2.imshow('cont',dispframe)
	return int(cx - window_x_half * 10)
	# cisimle kameranın orta noktasındaki farkı gönder