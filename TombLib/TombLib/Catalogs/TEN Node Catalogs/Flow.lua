﻿-- !Name "End level"
-- !Section "Game flow"
-- !Description "Ends current level and loads next level according to number. If number is 0, loads next level."
-- !Description "If number is more than level count, loads title."
-- !Arguments "Numerical, 15, [ 0 | 99 | 0], Level number"

LevelFuncs.EndLevel = function(number)
    Flow.EndLevel(number)
end

-- !Name "Add secret"
-- !Section "Game flow"
-- !Description "Adds one secret to game secret count and plays secret soundtrack."
-- !Arguments "Numerical, 15, [ 0 | 7 | 0 ], Level secret index"

LevelFuncs.AddSecret = function(number)
    Flow.AddSecret(number)
end

-- !Name "Set secret count"
-- !Section "Game flow"
-- !Description "Overwrites current game secret count with provided one."
-- !Arguments "Numerical, 15, [0 | 99 | 0], New secret count"

LevelFuncs.SetSecretCount = function(number)
    Flow.SetSecretCount(number)
end

-- !Name "If game secret count is..."
-- !Section "Game flow"
-- !Description "Checks current game secret count."
-- !Conditional "True"
-- !Arguments "CompareOperand, 25, Compare operation" "Numerical, 15, [0 | 99 | 0 ], Secret count"

LevelFuncs.GetSecretCount = function(operand, number)
    return LevelFuncs.CompareValue(Flow.GetSecretCount(), number, operand)
end