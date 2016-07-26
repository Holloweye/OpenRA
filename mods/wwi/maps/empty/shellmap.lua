
ticks = 99
speed = 5

FindRandomTarget = function()
  units = Map.ActorsInBox(Map.TopLeft, Map.BottomRight, function(actor)
    return actor.Owner == multi0 and actor.HasProperty("Location")
  end)

  if table.getn(units) > 0 then
    return Utils.Random(units)
  else
    return null
  end
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
		local unit0 = Actor.Create("WWI_RIFLE", true, { Owner = multi1, Location = FindRandomSpawnLocation() })
		local unit1 = Actor.Create("WWI_RIFLE", true, { Owner = multi1, Location = FindRandomSpawnLocation() })
		local unit2 = Actor.Create("WWI_GRENADIER", true, { Owner = multi1, Location = FindRandomSpawnLocation() })
		local unit3 = Actor.Create("WWI_RIFLE", true, { Owner = multi1, Location = FindRandomSpawnLocation() })
		local unit4 = Actor.Create("WWI_RIFLE", true, { Owner = multi1, Location = FindRandomSpawnLocation() })

    local location = FindRandomTarget().location

		unit0.AttackMove( location )
		unit1.AttackMove( location )
		unit2.AttackMove( location )
		unit3.AttackMove( location )
		unit4.AttackMove( location )
	end

  if ticks % 250 == 0 then
    units = Map.ActorsInBox(Map.TopLeft, Map.BottomRight, function(actor)
			return actor.Owner == multi1 and actor.HasProperty("Location")
		end)

    local location = FindRandomTarget().location
    Utils.Do(units, function(unit)
			unit.AttackMove( location )
		end)
  end

end

WorldLoaded = function()

  multi0 = Player.GetPlayer("Multi0")
  multi1 = Player.GetPlayer("Multi1")

  units = Map.ActorsInBox(Map.TopLeft, Map.BottomRight, function(actor)
    return actor.Owner == Player.GetPlayer("Multi0")
  end)

  Utils.Do(units, function(unit)
    unit.Patrol( {RandomLocationInBase()}, true, 0 )
  end)

end
