
WorldLoaded = function()
  bots = Player.GetPlayers(function(player)
    return player.IsBot == true and player.IsNonCombatant == false
  end)

  Utils.Do(bots, function(player)
    if player.Name == "Normal AI" then
      player.Cash = player.Cash * 10
    else
      player.Cash = player.Cash * 100
    end
  end)
end
