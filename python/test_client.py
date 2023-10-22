from DolunaySim import Dolunay
from test_mission import M1

mission = M1()

arac = Dolunay()

arac.set_arm(True)

arac.set_mod('ACRO')
# Gorev algoritmasını burada calıstıracagız
while True:
	move = mission.FindRed(arac.sim_data['cam_1'], arac.sim_data['cam_2'])

	data = arac.getData()
	print("________>")
	print(f"yaw: {data['yaw']} roll: {data['roll']} pitch: {data['pitch']} pressure: {data['pressure']}")
	print(f"right distance: {arac.Distance.getRightDistance()} left distance: {arac.Distance.getLeftDistance()} dif: {arac.Distance.getDiffDis()}")
	if move is not None:
		# print(f"mission move -> {move}")
		arac.hareket_et(*move, 1, 0)
	else:
		# print("search  move -> (0, -1000, 500, 0)")
		arac.hareket_et(0, -1000, 500, 0, 1, 0)