
ticks = 99
speed = 5

Tick = function()
	ticks = ticks + 1

	if ticks % 100 == 0 and ticks < 10000 then
		local unit0 = Actor.Create("WWI_RIFLE", true, { Owner = Player.GetPlayer("Multi1"), Location = CPos.New(85, 60) })
		local unit1 = Actor.Create("WWI_RIFLE", true, { Owner = Player.GetPlayer("Multi1"), Location = CPos.New(85, 61) })
		local unit2 = Actor.Create("WWI_GRENADIER", true, { Owner = Player.GetPlayer("Multi1"), Location = CPos.New(85, 62) })
		local unit3 = Actor.Create("WWI_RIFLE", true, { Owner = Player.GetPlayer("Multi1"), Location = CPos.New(85, 63) })
		local unit4 = Actor.Create("WWI_RIFLE", true, { Owner = Player.GetPlayer("Multi1"), Location = CPos.New(85, 64) })

		unit0.AttackMove( CPos.New(49, 63) )
		unit1.AttackMove( CPos.New(49, 63) )
		unit2.AttackMove( CPos.New(49, 63) )
		unit3.AttackMove( CPos.New(49, 63) )
		unit4.AttackMove( CPos.New(49, 63) )
	end
end
