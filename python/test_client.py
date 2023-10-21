from DolunaySim import Dolunay
from test_mission import M1

mission = M1()

arac = Dolunay()

arac.set_arm(True)

arac.set_mod('ACRO')
try:
	# Gorev algoritmasını burada calıstıracagız
	while True:
		move = mission.FindRed(arac.sim_data['cam_1'], arac.sim_data['cam_2'])

		if move is not None:
			print("move _>")
			print(move)
			arac.hareket_et(*move, 1, 0)
		else:
			arac.hareket_et(0, -1000, 500, 0, 1, 0)

except KeyboardInterrupt:
	# Ctrl - C basarak görevi bitir
	pass

arac.set_arm(0)

arac.kapat()