
ticks = 99
speed = 5

multi0 = null
multi1 = null

GetMulti0 = function()
  if multi0 == null then
    multi0 = Player.GetPlayer("Multi0")
  end
  return multi0
end

GetMulti1 = function()
  if multi1 == null then
    multi1 = Player.GetPlayer("Multi1")
  end
  return multi1
end

FindRandomSpawnLocation = function()
  return CPos.New(Utils.RandomInteger(87, 94), Utils.RandomInteger(54, 72))
end

RandomLocationInBase = function()
  return CPos.New(Utils.RandomInteger(50, 65), Utils.RandomInteger(60, 67))
end

Tick = function()
	ticks = ticks + 1

	if ticks % 100 == 0 and ticks < 5000 then
		local unit0 = Actor.Create("WWI_RIFLE", true, { Owner = GetMulti1(), Location = FindRandomSpawnLocation() })
		local unit1 = Actor.Create("WWI_RIFLE", true, { Owner = GetMulti1(), Location = FindRandomSpawnLocation() })
		local unit2 = Actor.Create("WWI_GRENADIER", true, { Owner = GetMulti1(), Location = FindRandomSpawnLocation() })
		local unit3 = Actor.Create("WWI_RIFLE", true, { Owner = GetMulti1(), Location = FindRandomSpawnLocation() })
		local unit4 = Actor.Create("WWI_RIFLE", true, { Owner = GetMulti1(), Location = FindRandomSpawnLocation() })

    local location = RandomLocationInBase()

		unit0.AttackMove( location )
		unit1.AttackMove( location )
		unit2.AttackMove( location )
		unit3.AttackMove( location )
		unit4.AttackMove( location )
	end

  if ticks % 250 == 0 then
    units = Map.ActorsInBox(Map.TopLeft, Map.BottomRight, function(actor)
			return actor.Owner == GetMulti1() and actor.HasProperty("Location")
		end)

    local location = RandomLocationInBase()
    Utils.Do(units, function(unit)
			unit.AttackMove( location )
		end)
  end

  local t = (ticks + 45) % (360 * speed) * (math.pi / 180) / speed;
	Camera.Position = viewportOrigin + WVec.New(-5000 * math.sin(t), 5000 * math.cos(t), 0)

end

WorldLoaded = function()
	viewportOrigin = Camera.Position
end
