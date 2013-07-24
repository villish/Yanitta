--------------------------------------------------------------------------------------------------
--									Nova Functions												--
--------------------------------------------------------------------------------------------------
PQR_Spec = GetSpecialization()
PQR_LevelInfo = UnitLevel("player")

Nova_SpellAvailableTime = nil
function Nova_SpellAvailableTime()
	local lag = ((select(3,GetNetStats()) + select(4,GetNetStats())) / 1000)
	if lag < .05 then
		lag = .05
	elseif lag > .4 then
		lag = .4
	end
	return lag
end

Nova_UnitInfo = nil
function Nova_UnitInfo(t)
	-- Takes an input of UnitID (player, target, pet, mouseover, etc) and gives you their most useful info
		local TManaActual = UnitPower(t)
		local TMaxMana = UnitPowerMax(t)
		if TMaxMana == 0 then TMaxMana = 1 end
		local TMana = 100 * UnitPower(t) / TMaxMana
		local THealthActual = UnitHealth(t)
		local THealth = 100 * UnitHealth(t) / UnitHealthMax(t)
		local myClassPower = 0
		local PQ_Class = select(2, UnitClass(t))
		local PQ_UnitLevel = UnitLevel(t)
		local PQ_CombatCheck = UnitAffectingCombat(t)
		if PQ_Class == "PALADIN" then
			myClassPower = UnitPower("player", 9)
			if UnitBuffID("player", 90174) then
				myClassPower = myClassPower + 3
			end
		elseif PQ_Class == "PRIEST" then
			myClassPower = UnitPower("player", 13)
		elseif PQ_Class == "WARLOCK" then
			if PQR_Spec == 3 then
				myClassPower = UnitPower("player", 14) -- Destruction: Burning Embers
			elseif PQR_Spec == 2 then
				myClassPower = UnitPower("player", 15) -- Demonology: Demonic Fury
			elseif PQR_Spec == 1 then
				myClassPower = UnitPower("player", 7) -- Affliction: Soul Shards
			end
		elseif PQ_Class == "DRUID" and PQ_Class == 2 then
			myClassPower = UnitPower("player", 8)
		elseif PQ_Class == "MONK"  then
			myClassPower = UnitPower("player", 12)
		elseif PQ_Class == "ROGUE" and t ~= "player" then
			myClassPower = GetComboPoints("player", t)
		end
		--       1            2          3         4           5             6          7               8
		return THealth, THealthActual, TMana, TManaActual, myClassPower, PQ_Class, PQ_UnitLevel, PQ_CombatCheck
end

--Tabled Cast Time Checking for When you Last Cast Something.
CheckCastTime = {}
Nova_CheckLastCast = nil
function Nova_CheckLastCast(spellid, ytime) -- SpellID of Spell To Check, How long of a gap are you looking for?
	if ytime > 0 then
		if #CheckCastTime > 0 then
			for i=1, #CheckCastTime do
				if CheckCastTime[i].SpellID == spellid then
					if GetTime() - CheckCastTime[i].CastTime > ytime then
						CheckCastTime[i].CastTime = GetTime()
						return true
					else
						return false
					end
				end
			end
		end
		table.insert(CheckCastTime, { SpellID = spellid, CastTime = GetTime() } )
		return true
	elseif ytime <= 0 then
		return true
	end
	return false
end

Nova_CustomT = { }

----------------------------------------------
-- Sheuron Healing Functions
----------------------------------------------
function CalculateHP(t)
	incomingheals = UnitGetIncomingHeals(t) and UnitGetIncomingHeals(t) or 0
	local PercentWithIncoming = 100 * ( UnitHealth(t) + incomingheals ) / UnitHealthMax(t)
	local ActualWithIncoming = ( UnitHealthMax(t) - ( UnitHealth(t) + incomingheals ) )
	return PercentWithIncoming, ActualWithIncoming
end

function CanHeal(t)
	if not UnitIsCharmed(t)
		and UnitInRange(t)
		and UnitIsConnected(t)
		and UnitCanCooperate("player",t)
		and not LineOfSight(t)
		and not UnitIsDeadOrGhost(t)
		and not PQR_IsOutOfSight(t)
		and UnitDebuffID(t,104451) == nil -- Ice Tomb
		and UnitDebuffID(t,76577) == nil -- Smoke Bomb
		and UnitDebuffID(t,121949) == nil -- Parasistic Growth
		and UnitDebuffID(t,122784) == nil -- Reshape Life
		and UnitDebuffID(t,122370) == nil -- Reshape Life 2
		and UnitDebuffID(t,123184) == nil -- Dissonance Field
		and UnitDebuffID(t,123255) == nil -- Dissonance Field 2
		and UnitDebuffID(t,123596) == nil -- Dissonance Field 3
		and UnitDebuffID(t,128353) == nil -- Dissonance Field 4
		and UnitDebuffID(t,128353) == nil -- Dissonance Field 4
		and UnitDebuffID(t,137341) == nil -- Beast of Nightmares
		and UnitDebuffID(t,137360) == nil -- Corrupted Healing - ToT
		and UnitDebuffID(t,23402) == nil -- Corrupted Healing - DS
		and UnitDebuffID(t,140701) == nil -- Crystal Shell: Full Capacity! - Tortos HC
		then
			if UnitDebuffID("player",116260) then
				if UnitDebuffID(t,116260) then
					return true
				else
					return false
				end
			elseif UnitDebuffID("player",116161) then
				if UnitDebuffID(t,116161) then
					return true
				else
					return false
				end
			else
				if UnitDebuffID(t,116260) or UnitDebuffID(t,116161) then
					return false
				else
					return true
				end
			end
		else
			return false
		end
end

function SheuronEngine(MO, LOWHP, ACTUALHP, TARGETHEAL)
	Nova_Tanks = { }
	local MouseoverCheck = MO or false
	local ActualHP = ACTUALHP or false
	local LowHPTarget = LOWHP or 80
	local TargetHealCheck = TARGETHEAL or true
	lowhpmembers = 0
	members = { { Unit = "player", HP = CalculateHP("player"), GUID = UnitGUID("player"), AHP = select(2, CalculateHP("player")) } }

	-- Check if the Player is apart of the Custom Table
	for i=1, #Nova_CustomT do
		if UnitGUID("player") == Nova_CustomT[i].GUID then
			Nova_CustomT[i].Unit = "player"
			Nova_CustomT[i].HP = CalculateHP("player")
			Nova_CustomT[i].AHP = select(2, CalculateHP("player"))
		end
	end

	if IsInRaid() then
			group = "raid"
	elseif IsInGroup() then
			group = "party"
	end

	for i = 1, GetNumGroupMembers() do
		local member, memberhp = group..i, CalculateHP(group..i)

		-- Checking all Party/Raid Members for Range/Health
		if CanHeal(member) then
			-- Checking if Member has threat
			if UnitThreatSituation(member) == 3 then memberhp = memberhp - 1 end
			-- Checking if Member has Beacon on them
			if UnitBuffID(member, 53563) then memberhp = memberhp + 3 end
			-- Searing Plasma Check
			if UnitDebuffID(member, 109379) then memberhp = memberhp - 9 end
			-- Volatile Pathogen check
			if UnitDebuffID(member, 136228) then memberhp = memberhp - 9 end
			-- Checking if Member is a tank
			if UnitGroupRolesAssigned(member) == "TANK" then
				memberhp = memberhp - 1
				if member ~= nil and memberhp ~= nil and select(2, CalculateHP(member)) ~= nil then
					table.insert(Nova_Tanks, { Unit = member, HP = memberhp, AHP = select(2, CalculateHP(member)) } )
				end
			end
			-- If they are in the Custom Table add their info in
			for i=1, #Nova_CustomT do
				if UnitGUID(member) == Nova_CustomT[i].GUID then
					Nova_CustomT[i].Unit = member
					Nova_CustomT[i].HP = memberhp
					Nova_CustomT[i].AHP = select(2, CalculateHP(member))
				end
			end
			if group..i ~= nil and memberhp ~= nil and UnitGUID(group..i) ~= nil and select(2, CalculateHP(group..i)) ~= nil then
				table.insert( members,{ Unit = group..i, HP = memberhp, GUID = UnitGUID(group..i), AHP = select(2, CalculateHP(group..i)) } )
			end
		end

		-- Checking Pets in the group
		if CanHeal(group..i.."pet") then
			local memberpet, memberpethp = nil, nil
			if UnitAffectingCombat("player") then
				 memberpet = group..i.."pet"
				 memberpethp = CalculateHP(group..i.."pet") * 2
			else
				 memberpet = group..i.."pet"
				 memberpethp = CalculateHP(group..i.."pet")
			end

			-- Checking if Pet is apart of the CustomTable
			for i=1, #Nova_CustomT do
				if UnitGUID(memberpet) == Nova_CustomT[i].GUID then
					Nova_CustomT[i].Unit = memberpet
					Nova_CustomT[i].HP = memberpethp
					Nova_CustomT[i].AHP = select(2, CalculateHP(memberpet))
				end
			end
			if memberpet ~= nil and memberpethp ~= nil and UnitGUID(memberpet) ~= nil and select(2, CalculateHP(memberpet)) ~= nil then
				table.insert(members, { Unit = memberpet, HP = memberpethp, GUID = UnitGUID(memberpet), AHP = select(2, CalculateHP(memberpet)) } )
			end
		end
	end

	-- So if we pass that ActualHP is true, then we will sort by most health missing. If not, we sort by lowest % of health.
	if not ActualHP then
		table.sort(members, function(x,y) return x.HP < y.HP end)
		if #Nova_Tanks > 0 then
			table.sort(Nova_Tanks, function(x,y) return x.HP < y.HP end)
		end
	elseif ActualHP then
		table.sort(members, function(x,y) return x.AHP > y.AHP end)
		if #Nova_Tanks > 0 then
			table.sort(Nova_Tanks, function(x,y) return x.AHP > y.AHP end)
		end
	end

	-- Setting Low HP Members variable for AoE Healing
	for i=1,#members do
		if members[i].HP < LowHPTarget then
			lowhpmembers = lowhpmembers + 1
		end
	end

	-- Checking Priority Targeting
	if CanHeal("target") and TargetHealCheck then
		table.sort(members, function(x) return UnitIsUnit("target",x.Unit) end)
	elseif CanHeal("mouseover") and GetMouseFocus() ~= WorldFrame and MouseoverCheck then
		table.sort(members, function(x) return UnitIsUnit("mouseover",x.Unit) end)
	end
end

function UnitsClose(t, percent)
	local PercentToHeal = percent or 85
	local n = 0

	if distance and distance[1] then
		for i=1,#members do
			local x = CheckDistance(t,members[i].Unit)
			if x ~= 0 and x < distance[1] and members[i].HP < PercentToHeal then n = n + 1 end
		end
	end
	return n
end

function GetDistance()
	local playerx,playery = GetPlayerMapPosition("player")

	if GetCurrentMapAreaID() ~= xrnMap or GetCurrentMapDungeonLevel() ~= xrnDung then
		xrnMap,xrnDung = GetCurrentMapAreaID(), GetCurrentMapDungeonLevel()
		mp, distance = {}, {}
	end

	if #members > 1 and #distance < 10 and playerx ~= 0 and playery ~= 0 then
		for i=1,#members do
			if CheckInteractDistance(members[i].Unit,2) then
				mp[members[i].Unit] = {GetPlayerMapPosition(members[i].Unit)}
			elseif UnitInRange(members[i].Unit) and mp[members[i].Unit] then
				table.insert(distance,sqrt((mp[members[i].Unit][1] - playerx)^2 + (mp[members[i].Unit][2] - playery)^2))
				table.sort(distance)
				mp[members[i].Unit] = nil
			end
		end
	end
end

function PQR_UnitDistance(var1, var2)
	local distance = 50
	local a,b,c,d,e,f,g,h,i,j = GetAreaMapInfo(GetCurrentMapAreaID())
	--if a and b and c and d and e and f and g and h and i and j then
	if a ~= nil and b ~= nil and c ~= nil and d ~= nil and e ~= nil and f ~= nil and g ~= nil and h ~= nil and i ~= nil and j ~= nil then
		local x1 , y1 = PQR_UnitInfo(var1)
		local x2 , y2 = PQR_UnitInfo(var2)
		--if x1 and x2 and y1 and y2 then
		if x1 ~= nil and x2 ~= nil and y1 ~= nil and y2 ~= nil then
			local w = (d - e)
			local h = (f - g)
			local distance = sqrt(min(x1 - x2, w - (x1 - x2))^2 + min(y1 - y2, h - (y1-y2))^2)
			--PQR_WriteToChat("\124cFFFF55FFDistance: "..distance)
			return distance
		end
	end
	return distance
end

if not tLOS then tLOS={} end
if not fLOS then fLOS=CreateFrame("Frame") end

function LineOfSight(target)
	local updateRate=3
	--local x1, y1 = PQR_UnitInfo(target)
	fLOS:RegisterEvent("COMBAT_LOG_EVENT_UNFILTERED")
	function fLOSOnEvent(self,event,...)
		if event=="COMBAT_LOG_EVENT_UNFILTERED" then
			local _, subEvent, _, sourceGUID, _, _, _, _, _, _, _, _, _, _, spellFailed  = ...
			if subEvent ~= nil then
				if subEvent=="SPELL_CAST_FAILED" then
					local player=UnitGUID("player") or ""
					if sourceGUID ~= nil then
						if sourceGUID==player then
							if spellFailed ~= nil then
								if spellFailed==SPELL_FAILED_LINE_OF_SIGHT
								or spellFailed==SPELL_FAILED_NOT_INFRONT
								or spellFailed==SPELL_FAILED_OUT_OF_RANGE
								or spellFailed==SPELL_FAILED_UNIT_NOT_INFRONT
								or spellFailed==SPELL_FAILED_UNIT_NOT_BEHIND
								or spellFailed==SPELL_FAILED_NOT_BEHIND
								or spellFailed==SPELL_FAILED_MOVING
								or spellFailed==SPELL_FAILED_IMMUNE
								or spellFailed==SPELL_FAILED_FLEEING
								or spellFailed==SPELL_FAILED_BAD_TARGETS
								--or spellFailed==SPELL_FAILED_NO_MOUNTS_ALLOWED
								or spellFailed==SPELL_FAILED_STUNNED
								or spellFailed==SPELL_FAILED_SILENCED
								or spellFailed==SPELL_FAILED_NOT_IN_CONTROL
								or spellFailed==SPELL_FAILED_VISION_OBSCURED
								or spellFailed==SPELL_FAILED_DAMAGE_IMMUNE
								or spellFailed==SPELL_FAILED_CHARMED
								then
									--tinsert(tLOS,{unit=target,time=GetTime(),x=x1,y=y1})
									tLOS={}
									tinsert(tLOS,{unit=target,time=GetTime()})
								end
							end
						end
					end
				end
			end

			if #tLOS > 0 then
				table.sort(tLOS,function(x,y) return x.time>y.time end)
				if (GetTime()>(tLOS[1].time+updateRate)) then
					tLOS={}
				end
			end
		end
	end
	fLOS:SetScript("OnEvent",fLOSOnEvent)
	if #tLOS > 0 then
		if tLOS[1].unit==target
		--and (tLOS[i].x - 5) <= x1 and (tLOS[i].x + 5) >= x1 and (tLOS[i].y - 5) <= y1 and (tLOS[i].y + 5) >= y1
		then
			--PQR_WriteToChat("\124cFFFF55FFLoS Name: "..UnitName(target))
			return true
		end
	end
end

--------------------------------------------------------------------------------------------------
--									Vachiusa Functions											--
--------------------------------------------------------------------------------------------------

  -- Checks if our Cleanse will have a valid Debuff to Cleanse
function ValidDispel(t)
  	local HasValidDispel = false
  	local i = 1
  	local debuff = UnitDebuff(t, i)
  	while debuff do
  		local debuffType = select(5, UnitDebuff(t, i))
  		local debuffid = select(11, UnitDebuff(t, i))
  		local PQ_Class = select(2, UnitClass(t))
  		local ValidDebuffType = false
		if PQ_Class == "PALADIN" then
  			if debuffType == "Magic" or debuffType == "Poison" or debuffType == "Disease" then
  				ValidDebuffType = true
  			end
		elseif PQ_Class == "DRUID" then
  			if debuffType == "Magic" or debuffType == "Poison" or debuffType == "Curse" then
  				ValidDebuffType = true
  			--elseif PQR_SpellAvailable(122288) and debuffType == "Disease" then --Cleanse from Paladin Symbiosis
  				--ValidDebuffType = true
  			end
		elseif PQ_Class == "MONK" then
  			if debuffType == "Magic" or debuffType == "Poison" or debuffType == "Disease" then
  				ValidDebuffType = true
  			end
		elseif PQ_Class == "PRIEST" then
  			if debuffType == "Magic" or debuffType == "Disease" then
  				ValidDebuffType = true
  			end
		elseif PQ_Class == "SHAMAN" then
  			if debuffType == "Magic" or debuffType == "Curse" then
  				ValidDebuffType = true
  			end
  		end

  		if ValidDebuffType
  		--and debuffid ~= 138732 --Ionization from Jin'rokh the Breaker - ptr
		--and debuffid ~= 138733 --Ionization from Jin'rokh the Breaker - live
		then
  			HasValidDispel = true
  		end
  		i = i + 1
  		debuff = UnitDebuff(t, i)
  	end
	return HasValidDispel
end

-- Custom CalStop function
function CalStop(n)
	local myIncomingHeal = UnitGetIncomingHeals(n, "player") or 0
	local allIncomingHeal = UnitGetIncomingHeals(n) or 0
	local overheal = 0
	if myIncomingHeal >= allIncomingHeal then
		overheal = 0
	else
		overheal = allIncomingHeal - myIncomingHeal
	end
	local overhealth = 100 * (UnitHealth(n)+ overheal ) / UnitHealthMax(n)
	return overhealth, overheal
end

-- Average Health of Players
function AverageHealth(n) -- N = Size of the range of people we are checking
	local NumberOfPeople = n
	local Nova_Average = 0
	if #members < NumberOfPeople then
		for i=NumberOfPeople, 0, -1 do
			if #members >= i then
				NumberOfPeople = i
				break
			end
		end
	end

	for i=1, NumberOfPeople do
		Nova_Average = Nova_Average + members[i].HP
	end

	Nova_Average = Nova_Average / NumberOfPeople

	return Nova_Average, NumberOfPeople
end

--Custom Unit Facing check
function VUnitFacingCheck(act,t)
	local act = act or true
	if not act then
		return true
	end
	local t = t or nil

	if not HaveTank then
		function HaveTank()
			if #Nova_Tanks > 0 then
				for i=1, #Nova_Tanks do
					if CheckInteractDistance(Nova_Tanks[i].Unit, 4) then
						return Nova_Tanks[i].Unit
					end
				end
			end
		end
	end

	if HaveTank() then
		local px,py = GetPlayerMapPosition("player")
		local tx,ty = GetPlayerMapPosition(HaveTank())
		local angle = floor ( ( math.pi - math.atan2(px-tx,ty-py) - GetPlayerFacing() ) / (math.pi*2) * 32 + 0.5 ) % 32
		if px ~= 0 and tx ~= 0 then
			if angle > 0 and angle < 16 then
				return false
			end
			if angle > 15 and angle < 31 then
				return false
			end
	    	if angle == 31 or angle == 0 then
				return true
	    	else
	    		return false
	    	end
		else
			return false
		end
	else
		return false
	end
end

--Custom GetDistance raid/party member
function PRGetDistance(t,thp,mhp,d,b) --t: "player", "partyN" or "raidN" and not pets, thp: target HP, mhp: hp member for collect, d: distance in yard, b: number of distance requirement
	local real_b = 0
	if #members > 1 then
		for i=1,#members do
			if PQR_UnitDistance(t,members[i].Unit) and members[i].HP then
				if PQR_UnitDistance(t,members[i].Unit) <= d and not UnitIsUnit(t,members[i].Unit) and members[i].HP <= mhp then
					real_b = real_b + 1
					--if real_b >= b then
						--break
					--end
				end
			end
		end
		if real_b > 0 and UnitInRange(t) then --UnitInRange = 40y
			table.insert(prdistance, { Unit = t, HP = thp, PD = real_b } )
			table.sort(prdistance, function(x,y) return x.PD < y.PD end)
		end
	end
end

function PRGetDistanceTable(mhp,d,b)
	if not d then local d = 10 end
	if not b then local b = 25 end
	if not mhp then local mhp = 95 end
	prdistance = { { Unit = "player", HP = CalculateHP("player"), PD = 0 } }
	if #members > 1 then
		for i=1,#members do
			if members[i].HP then
				if members[i].HP <= mhp and UnitInRange(members[i].Unit) then
					PRGetDistance(members[i].Unit, members[i].HP, mhp, d, b)
					--if prdistance[1].PD >= b then
						--break
					--end
					--EX: PRGetDistanceTable(95, 20, 7) --ChiWave
				end
			end
		end
	end
end

--Custom sort by HP
function PRGetDistanceTablebyHP(h,b)
	prdistancebyhp = { { Unit = "player", HP = CalculateHP("player"), PD = 0 } }
	if #prdistance > 1 then
		for i=1,#prdistance do
			if prdistance[i].PD and prdistance[i].HP then
				if prdistance[i].PD >= b and prdistance[i].HP <= h then
					table.insert(prdistancebyhp, { Unit = prdistance[i].Unit, HP = prdistance[i].HP, PD = prdistance[i].PD } )
					table.sort(prdistancebyhp, function(x,y) return x.HP < y.HP end)
				end
			end
		end
	end
end

--Custom GetDistance raid/party member with buff
function PRGetDistancebuff(t,thp,mhp,d,b,buff) --t: "player", "partyN" or "raidN" and not pets, thp: target HP, mhp: hp member for collect, d: distance in yard, b: number of distance requirement
	local real_b = 0
	if #members > 1 then
		for i=1,#members do
			if members[i].HP and PQR_UnitDistance(t,members[i].Unit) then
				if PQR_UnitDistance(t,members[i].Unit) <= d and not UnitIsUnit(t,members[i].Unit) and members[i].HP <= mhp and not UnitBuffID(members[i].Unit,buff,"player") then
					real_b = real_b + 1
					--if real_b >= b then
						--break
					--end
				end
			end
		end
		if real_b > 0 and UnitInRange(t) then --UnitInRange = 40y
			table.insert(prdistancebuff, { Unit = t, HP = thp, PD = real_b } )
			table.sort(prdistancebuff, function(x,y) return x.PD < y.PD end)
		end
	end
end

function PRGetDistanceTablebuff(mhp,d,b,buff)
	if not d then local d = 10 end
	if not b then local b = 25 end
	if not mhp then local mhp = 95 end
	if not buff then local buff = 115151 end --RM
	prdistancebuff = { { Unit = "player", HP = CalculateHP("player"), PD = 0 } }
	if #members > 1 then
		for i=1,#members do
			if members[i].HP then
				if members[i].HP <= mhp and UnitInRange(members[i].Unit) and not UnitBuffID(members[i].Unit,buff,"player") then
					PRGetDistancebuff(members[i].Unit, members[i].HP, mhp, d, b, buff)
					--if prdistance[1].PD >= b then
						--break
					--end
					--EX: PRGetDistanceTable(95, 20, 7) --ChiWave
				end
			end
		end
	end
end

--Custom sort by HP with buff
function PRGetDistanceTablebyHPbuff(h,b)
	prdistancebyhpbuff = { { Unit = "player", HP = CalculateHP("player"), PD = 0 } }
	if #prdistancebuff > 1 then
		for i=1,#prdistancebuff do
			if prdistancebuff[i].PD and prdistancebuff[i].HP then
				if prdistancebuff[i].PD >= b and prdistancebuff[i].HP <= h then
					table.insert(prdistancebyhpbuff, { Unit = prdistancebuff[i].Unit, HP = prdistancebuff[i].HP, PD = prdistancebuff[i].PD } )
					table.sort(prdistancebyhpbuff, function(x,y) return x.HP < y.HP end)
				end
			end
		end
	end
end

--Druid
--Custom GetDistance raid/party member
function PRGetDistance2(t,thp,mhp,d,b) --t: "player", "partyN" or "raidN" and not pets, thp: target HP, mhp: hp member for collect, d: distance in yard, b: number of distance requirement
	local real_b = 0
	if #members > 1 then
		for i=1,#members do
			if PQR_UnitDistance(t,members[i].Unit) and members[i].HP then
				if PQR_UnitDistance(t,members[i].Unit) <= d and not UnitIsUnit(t,members[i].Unit) and members[i].HP <= mhp then
					real_b = real_b + 1
				end
			end
		end
		if real_b > 0 and UnitInRange(t) then --UnitInRange = 40y
			table.insert(prdistance2, { Unit = t, HP = thp, PD = real_b } )
			table.sort(prdistance2, function(x,y) return x.PD < y.PD end)
		end
	end
end

function PRGetDistanceTablebuff2(mhp,d,b,buff1,buff2)
	if not d then local d = 8 end
	if not b then local b = 25 end
	if not mhp then local mhp = 95 end
	if not buff1 then local buff1 = 8936 end
	if not buff2 then local buff2 = 774 end
	prdistance2 = { { Unit = "player", HP = CalculateHP("player"), PD = 0 } }
	if #members > 1 then
		for i=1,#members do
			if members[i].HP then
				if members[i].HP <= mhp and UnitInRange(members[i].Unit) and (UnitBuffID(members[i].Unit,buff1) or UnitBuffID(members[i].Unit,buff2)) then
					PRGetDistance2(members[i].Unit, members[i].HP, mhp, d, b)
				end
			end
		end
	end
end

--Custom sort by HP with buff
function PRGetDistanceTablebyHPbuff2(h,b)
	prdistancebyhp2 = { { Unit = "player", HP = CalculateHP("player"), PD = 0 } }
	if #prdistance2 > 1 then
		for i=1,#prdistance2 do
			if prdistance2[i].PD and prdistance2[i].HP then
				if prdistance2[i].PD >= b and prdistance2[i].HP <= h then
					table.insert(prdistancebyhp2, { Unit = prdistance2[i].Unit, HP = prdistance2[i].HP, PD = prdistance2[i].PD } )
					table.sort(prdistancebyhp2, function(x,y) return x.HP < y.HP end)
				end
			end
		end
	end
end

-- Target & Environmental Globals and Tables
-------------------------------------------------------------------------------
PQ_Shrapnel			= {106794,106791}
PQ_FadingLight		= {105925,105926,109075,109200}
PQ_HourOfTwilight	= {106371,103327,106389,106174,106370}

--------------------------------------- ABILITY --------------------------

-- Healing Engine --
SheuronEngine(Nova_Mouseover, Nova_LowHP, Nova_ActulHP)  -- Deactivate Mouseover  ||   At what % Health do we consider someone LowHP  ||  Sort by Actual Health = true
----------------------------------------
-- Init --
if PQR_RotationStarted == true then
	-- Should be reloaded every time you reload Profile (No more needing to /rl)
 	-- Only takes full effects when we're 90
 	if UnitLevel("player") ~= 90 then
 		PQR_WriteToChat("\124cFFFF55FFWarning: only takes full effect when we're 90!")
 	end
	PQR_RotationStarted = false

PQR_Event("PQR_Text", "Resto Druid Profile Verion - 2.0.12", nil, "00FF00")
PQR_SwapCheckTimer = 0

--------------------
-- Register CVars
--------------------

Nova_ValueCheck = {
	{	Var1 = nil,	Text = "Healing Values",			Var2 = nil		},
	{	Var1 = 60,	Text = "VHealingTouch",				Var2 = 1		},
	{	Var1 = 80,	Text = "VOmenHealingTouch",			Var2 = 1		},
	{	Var1 = 80,	Text = "VNourish",					Var2 = 1		},
	{	Var1 = 40,	Text = "VRegrowth",					Var2 = 1		},
	{	Var1 = 75,	Text = "VOmenRegrowth",				Var2 = 1		},
	{	Var1 = 80,	Text = "VRejuvenation",				Var2 = 1		},
	{	Var1 = 90,	Text = "VRejuvenationTank",			Var2 = 1		},
	{	Var1 = 85,	Text = "VSwiftmend",				Var2 = 1		},
	{	Var1 = 2,	Text = "VSwiftmendLimit",			Var2 = 1		},
	{	Var1 = 85,	Text = "VWildGrowth",				Var2 = 1		},
	{	Var1 = 3,	Text = "VWildGrowthLimit",			Var2 = 1		},
	{	Var1 = nil,	Text = "Cooldown Values",			Var2 = nil		},
	{	Var1 = 20,	Text = "VIronbark",					Var2 = 1		},
	--{	Var1 = 40,	Text = "VNatureSwiftness",			Var2 = 1		},
	{	Var1 = 40,	Text = "VSpiritWalkerGrace",		Var2 = 1		},
	{	Var1 = 60,	Text = "VCooldowns",				Var2 = 1		},
	{	Var1 = 4,	Text = "VCooldownsLimit",			Var2 = 1		},
	{	Var1 = 0,	Text = "VTalent90Auto",				Var2 = 1		},
	{	Var1 = 0,	Text = "VIncarnationAuto",			Var2 = 1		},
	{	Var1 = 0,	Text = "VTranquilityAuto",			Var2 = 1		},
	{	Var1 = nil,	Text = "Misc Values",				Var2 = nil		},
	{	Var1 = 90,	Text = "VDPS",						Var2 = 0		},
	{	Var1 = 0,	Text = "VAutoTarget",				Var2 = 1		},
	{	Var1 = 20,	Text = "VLowMana",					Var2 = 1		},
	{	Var1 = 60,	Text = "VNaturesCure",				Var2 = 1		},
	{	Var1 = 80,	Text = "VInnervate",				Var2 = 1		},
	--{	Var1 = 0,	Text = "VSmart",					Var2 = 1		},
	{	Var1 = 0,	Text = "VSymbiosis",				Var2 = 1		},
	{	Var1 = 0,	Text = "Racials",					Var2 = 1		},
	{	Var1 = 0,	Text = "NewEvents",					Var2 = 1		},
	{	Var1 = 0,	Text = "OldEvents",					Var2 = 0		}
	--{	Var1 = 80,	Text = "LowHPThreshold", 			Var2 = nil		}
}

Nova_CooldownCheck = {
	{	Mod = 8,	Text = "VIncarnation",				Var1 = 1		}, --IsRightShiftKeyDown
	{	Mod = 1,	Text = "VPauseRotation",			Var1 = 1		}, --IsLeftShiftKeyDown
	{	Mod = 2,	Text = "VWildMushroom",				Var1 = 1		}, --IsLeftControlKeyDown
	--{	Mod = 4,	Text = "VDispelRaid",				Var1 = 1		}, --IsLeftAltKeyDown
	{	Mod = 4,	Text = "VWildMushroomBloom",		Var1 = 1		}, --IsLeftAltKeyDown
	{	Mod = 32,	Text = "VMTranquility",				Var1 = 1		}, --IsRightAltKeyDown
	{	Mod = 16,	Text = "VTalent90",					Var1 = 1		}  --IsRightControlKeyDown
	--{	Mod = 2,	Text = "RemoveFromCustomTable",	Var1 = 1		}, --IsLeftControlKeyDown
	--{	Mod = 4,	Text = "AddToCustomTable",			Var1 = 1		}  --IsLeftAltKeyDown
}

if GetCVar("PQ_WipeCustomTable") == nil then RegisterCVar("PQ_WipeCustomTable", 0) end
if GetCVar("Nova_OverRide") == nil then RegisterCVar("Nova_OverRide", 0) end
if GetCVar("PQ_UseCustomT") == nil then RegisterCVar("PQ_UseCustomT", 0) end
if GetCVar("Nova_Recording") == nil then RegisterCVar("Nova_Recording", 0) end
if GetCVar("Nova_DisableCD") == nil then RegisterCVar("Nova_DisableCD", 1) end
if GetCVar("Nova_Mouseover") == nil then RegisterCVar("Nova_Mouseover", 0) end
if GetCVar("Nova_LowHP") == nil then RegisterCVar("Nova_LowHP", 80) end
if GetCVar("Nova_ActualHP") == nil then RegisterCVar("Nova_ActualHP", 0) end
-- Registering the CVars for the CustomFrame
for i=1, #Nova_ValueCheck do
	if GetCVar("Nova_"..Nova_ValueCheck[i].Text) == nil then
		RegisterCVar("Nova_"..string.gsub(Nova_ValueCheck[i].Text, "%s", "_"), Nova_ValueCheck[i].Var1)
	end
	if GetCVar("Nova_"..Nova_ValueCheck[i].Text..'_Enabled') == nil then
		RegisterCVar("Nova_"..string.gsub(Nova_ValueCheck[i].Text, "%s", "_")..'_Enabled', Nova_ValueCheck[i].Var2)
	end
end
for i=1, #Nova_CooldownCheck do
	if GetCVar("Nova_"..Nova_CooldownCheck[i].Text) == nil then
		RegisterCVar("Nova_"..Nova_CooldownCheck[i].Text, Nova_CooldownCheck[i].Mod)
	end
	if GetCVar("Nova_"..Nova_CooldownCheck[i].Text.."_Enabled") == nil then
		RegisterCVar("Nova_"..Nova_CooldownCheck[i].Text.."_Enabled", Nova_CooldownCheck[i].Var1)
	end
end

-- Variables
PQR_ResetMovementTime = 0.3
PQR_SpellAvailableTime = ((select(3,GetNetStats()) + select(4,GetNetStats())) / 1000)
PQR_AddToSpellDelayList(18562, 0, 1) -- Swiftmend
PQR_AddToSpellDelayList(44203, 0, 1) -- Tranquility

-----------------------------
-- Create the CVar Macros
-----------------------------

	if PQR_LoadLua ~= nil then
		-- Load Data File
		if PQR_LoadLua("PQR_Vachiusa_Data.lua") == false then
			PQR_WriteToChat("You are missing PQR_Vachiusa_Data.lua. Rotation has been stopped.", "Error")
			PQR_StopRotation()
			return true
		end

		if PQR_LoadLua("PQR_Vachiusa_Frame.lua") == true then
			if not mmC then
				mmC = true
				MiniMapCreation()
			end

			-- Setup the Slash Commands for the Frame
			SLASH_NOVAFRAME1 = "/novaframe"
			SLASH_NOVAFRAME2 = "/nova"
			function SlashCmdList.NOVAFRAME(msg, editbox)
				if Setup == nil then
					Setup = true
					FrameCreation(Nova_ValueCheck, Nova_CooldownCheck)
				end

				if not Nova_Frame:IsShown() then
					Nova_Frame:Show()
				else
					Nova_Frame:Hide()
				end
			end
		else
			print("Failed to load Frame")
		end
	end

	PQR_Spec = GetSpecialization()
	if PQR_Spec ~= 4 then
		PQR_WriteToChat("You must be in Resto Spec. Please switch then try again.", "Warning")
		PQR_StopRotation()
		return true
	end
end

if PQR_IsMoving() then
	Nova_Moving = true
else
	Nova_Moving = false
end

-------------------------------------------------
-- Master File --

----------------------------------------------------
-- Master Settings --
if not FirstRun and not GetCVarBool("Nova_OverRide") then
	FirstRun = true
	SetCVar("Nova_VRejuvenation",   85)
	SetCVar("Nova_VCooldowns",      60)
	SetCVar("Nova_VCooldownsLimit",  4)
	SetCVar("Nova_VSwiftmend",      85)
	SetCVar("Nova_VSwiftmendLimit",  2)
	SetCVar("Nova_VWildGrowth",     85)
	SetCVar("Nova_VWildGrowthLimit", 3)
end

---------------------------------------------------
-- NewEvents
if Nova_NewEventsCheck then
	local boss,bossid = bossid()
	local dispelid = 88423
	local buff = { }
	stopcasting = false
	stopfade = false
	LLdebuff = false
	LLdebuffunit = false
	if UnitExists(boss) then
		local _, _, rdifficulty = GetInstanceInfo()
		--TFT raid
		if bossid == 69465 and (rdifficulty == 5 or rdifficulty ==6) then --Jin'rokh the Breaker
			local buff = { 138732 } --Ionization
			local fluiditydebuff  = 138002 --Fluidity
			RaidJBDispel(dispelid,buff,fluiditydebuff,5)
		elseif bossid == 68905 or bossid == 68904 then --Lu'lin 68905, Suen 68904 - Twin Consorts
			local buff  = { 137360 } --Corrupted Healing
			LLdebuff,LLdebuffunit = RaidLLDispel(buff)
			if LLdebuff
			and UnitBuffID(LLdebuffunit, 33763, "PLAYER")
			and PQR_SpellAvailable(33763)
			and IsUsableSpell(33763) then
				CastSpellByName(tostring(GetSpellInfo(33763)),"player")
				return true
			end
		elseif bossid == 69134 or bossid == 69131 or bossid == 69078 or bossid == 69132 then --Council of Elders
			local buff  = { 136878, 136857 }
			RaidDispel(dispelid,buff)
		elseif bossid == 68476 then --Horridon
			--local buff  = { 136708, 136719, 136587, 136710, 136512 } --Magic, Magic, Poison, Disease, Curse
			local buff  = { 136708, 136719, 136587, 136512 }
			RaidDispel(dispelid,buff)
		elseif bossid == 68065 or bossid == 70212 or bossid == 70235 or bossid == 70247 then --Flaming Head
		-- Players affected by  Cinders should be quick to run over any existing  Icy Ground void zones, and then move to a safe location (out of the way of the raid) to be dispelled.
			local buff  = { 139822 }
			RaidRangeDispel(dispelid,buff,10)
		elseif bossid == 69427 then --Dark Animus
			local buff  = { 138609 }
			RaidDispelDelay(dispelid,buff,5)
			local InterruptingJolt = GetSpellInfo(138763) --139867
			local bossCasting,_,_,_,_,castEnd = UnitCastingInfo(boss)
			if (bossCasting == InterruptingJolt) then
				stopcasting = true
			end
		--DS raid
		elseif bossid == 53879 then --Blood Corruption: Death
			local buff  = { 106199 }
			RaidDispel(dispelid,buff)
		--TeS raid
		elseif bossid == 60585 or bossid == 60583 or bossid == 60586 then
			local buff  = { 117436 }
			RaidDispel(dispelid,buff) --Protectors of the Endless, Lightning Prison
		elseif bossid == 62442 then --Terrorize
			local buff  = { 123011 }
			RaidDispel(dispelid,buff)
		--MSV raid
		elseif bossid == 60051 or bossid == 60047 or bossid == 60043 or bossid == 59915 then --Cobalt Mine Blast
			local buff  = { 116281 }
			RaidDispel(dispelid,buff)
		elseif bossid == 60410 then --Closed Circuit
			local buff  = { 117949 }
			RaidDispel(dispelid,buff)
		--HoF raid
		elseif bossid == 62837 then --Visions of Demise
			local buff  = { 124863 }
			RaidDispel(dispelid,buff)
		--else
			--PQR_WriteToChat("\124cFFFF55FFBossid: "..bossid.." - boss: "..boss)
		end

		if bossid == 62442 then
			BossDispel(123011,dispelid,boss) --Tsulong, Terrorize
		end

		--MSV raid
		if bossid == 60143 then --not test
			-- Gara'jal the Spiritbinder
			if UnitDebuffID("player",116161) then
				local timer = select(7,UnitDebuffID("player",116161))
				if timer and timer - GetTime() < 1.5 then
					SpellStopCasting()
					RunMacroText("/click ExtraActionButton1")
				end
			end
		end
	end
end

-----------------------------------------------------------
-- Raid10 Settings --
if not FirstRun and not GetCVarBool("Nova_OverRide") then
	FirstRun = true
	SetCVar("Nova_VRejuvenation", 		85)
	SetCVar("Nova_VCooldowns", 			60)
	SetCVar("Nova_VCooldownsLimit", 	 8)
	SetCVar("Nova_VSwiftmend", 			85)
	SetCVar("Nova_VSwiftmendLimit", 	 2)
	SetCVar("Nova_VWildGrowth", 		80)
	SetCVar("Nova_VWildGrowthLimit", 	 5)
end

-------------------------------------------------------
-- Raid25 Settings --
if not FirstRun and not GetCVarBool("Nova_OverRide") then
	FirstRun = true
	SetCVar("Nova_VRejuvenation",      85)
	SetCVar("Nova_VCooldowns",         60)
	SetCVar("Nova_VCooldownsLimit",    16)
	SetCVar("Nova_VSwiftmend", 		   85)
	SetCVar("Nova_VSwiftmendLimit", 	2)
	SetCVar("Nova_VWildGrowth", 	   80)
	SetCVar("Nova_VWildGrowthLimit", 	5)
end

----------------------------------------
-- ToInteger --
-- To Integer from String
Nova_LowHP 							= tonumber( GetCVar("Nova_LowHP") )
for i=1, #Nova_ValueCheck do
	if Nova_ValueCheck[i].Var1 ~= nil then
		_G['Nova_'..string.gsub(Nova_ValueCheck[i].Text, '%s', '_')] = tonumber(GetCVar('Nova_'..string.gsub(Nova_ValueCheck[i].Text, '%s', '_')))
	end
	if Nova_ValueCheck[i].Var2 ~= nil then
		_G['Nova_'..string.gsub(Nova_ValueCheck[i].Text, '%s', '_')..'Check'] = GetCVarBool('Nova_'..string.gsub(Nova_ValueCheck[i].Text, '%s', '_')..'_Enabled')
	end
end

-- To Integer for Cooldowns
Nova_VIncarnation					= tonumber(	GetCVar('Nova_VIncarnation') )
Nova_VWildMushroomBloom				= tonumber(	GetCVar('Nova_VWildMushroomBloom') )
--Nova_VDispelRaid					= tonumber(	GetCVar('Nova_VDispelRaid') )
Nova_VMTranquility					= tonumber(	GetCVar('Nova_VMTranquility') )
Nova_VWildMushroom					= tonumber(	GetCVar('Nova_VWildMushroom') )
Nova_VTalent90						= tonumber(	GetCVar('Nova_VTalent90') )
--Nova_RemoveCT						= tonumber(	GetCVar('Nova_RemoveFromCustomTable') )
--Nova_AddCT						= tonumber(	GetCVar('Nova_AddToCustomTable') )

-- To Boolean from String
Nova_Mouseover 						= GetCVarBool("Nova_Mouseover")
Nova_VIncarnationCheck				= GetCVarBool('Nova_VIncarnation_Enabled')
Nova_VWildMushroomBloomCheck		= GetCVarBool('Nova_VWildMushroomBloom_Enabled')
--Nova_VDispelRaidCheck				= GetCVarBool('Nova_VDispelRaid_Enabled')
Nova_VMTranquilityCheck				= GetCVarBool('Nova_VMTranquility_Enabled')
Nova_VWildMushroomCheck				= GetCVarBool('Nova_VWildMushroom_Enabled')
Nova_VTalent90Check					= GetCVarBool('Nova_VTalent90_Enabled')
--Nova_RemoveCTCheck				= GetCVarBool('Nova_RemoveFromCustomTable_Enabled')
--Nova_AddCTCheck					= GetCVarBool('Nova_AddToCustomTable_Enabled')

-------------------------------------------------------
-- HealingTouch
--HealingTouch Целительное прикосновение
if Nova_VHealingTouchCheck then
	if PQR_SpellAvailable(5185)
	 and IsSpellInRange(GetSpellInfo(5185),members[1].Unit) == 1
	 and CanHeal(members[1].Unit)
	 and IsUsableSpell(5185)
	 and not Nova_Moving
	 and not stopcasting then
		if members[1].HP < Nova_VHealingTouch and select(3, Nova_UnitInfo("player")) > 20
		and not UnitCastingInfo("player") then
			--NatureSwiftness
			if not UnitBuffID("player",132158)
			and PQR_SpellAvailable(132158)
			and select(2,GetTalentRowSelectionInfo(2)) == 4
			and select(2,GetSpellCooldown(132158)) < 2
			and not UnitCastingInfo("player") then
				CastSpellByName(tostring(GetSpellInfo(132158)),"player")
				--PQR_WriteToChat("\124cFFFF55FFCast NatureSwiftness. HP: "..members[1].HP)
			end
			CastSpellByName(tostring(GetSpellInfo(5185)),members[1].Unit)
		 	--PQR_CustomTarget = members[1].Unit
		 	return true
		elseif not UnitCastingInfo("player")
		and UnitBuffID("player", 16870) and members[1].HP <= Nova_VOmenHealingTouch then --Omen of Clarity buff cost no mana
			CastSpellByName(tostring(GetSpellInfo(5185)),members[1].Unit)
		 	--PQR_CustomTarget = members[1].Unit
		 	return true
		end
	end
end
--------------------------------------------------------

--Ironbark Железная кора
if Nova_VIronbarkCheck then
	if PQR_SpellAvailable(102342)
	 and UnitAffectingCombat("player")
	 and select(2,GetSpellCooldown(102342)) < 2
	 --and GetSpellCooldown(33206) == 0
	 and UnitThreatSituation(members[1].Unit) == 3
	 and IsUsableSpell(102342)
	 and CanHeal(members[1].Unit)
	 --and not UnitCastingInfo("player")
	 and IsSpellInRange(GetSpellInfo(102342),members[1].Unit) == 1 then
		if members[1].HP < Nova_VIronbark
		and UnitIsPlayer(members[1].Unit)
		then
			if UnitCastingInfo("player") then
				SpellStopCasting()
			end
		 	--PQR_CustomTarget = members[1].Unit
		 	--PQR_WriteToChat("\124cFFFF55FFIronbark - HP: " ..members[1].HP)
		 	CastSpellByName(tostring(GetSpellInfo(102342)),members[1].Unit)
		 	return true
		end
	end
end
------------------------------------------------------
--Lifebloom - ToL support - need test
---------------------------------------------------------------------
--MarkOfTheWild Знак дикой природы
if not UnitBuffID("player",1126)
and not UnitBuffID("player",20217)
and not UnitBuffID("player",90363)
and not UnitBuffID("player",115921)
and PQR_SpellAvailable(1126)
and IsUsableSpell(1126)
and not UnitCastingInfo("player")
then
	CastSpellByName(tostring(GetSpellInfo(1126)),"player")
	return true
end

--------------------------------------------------------
--NaturesCure Природный целитель
if Nova_VDispelRaidCheck then
	if Nova_Mod() == Nova_VDispelRaid then
		if PQR_SpellAvailable(88423)
		and select(2,GetSpellCooldown(88423)) < 2
		--and IsLeftAltKeyDown()
		--and not IsLeftShiftKeyDown()
		and IsUsableSpell(88423)
		and not GetCurrentKeyBoardFocus()
		and not UnitChannelInfo("player") then
			if members[1].HP > Nova_VNaturesCure then --Nova_NaturesCure
				for i=1, #members do
					if ValidDispel(members[i].Unit)
					and CanHeal(members[i].Unit)
					and IsSpellInRange(GetSpellInfo(88423),members[i].Unit) == 1
					and not UnitCastingInfo("player") then
						PQR_CustomTarget = members[i].Unit
						CastSpellByName(tostring(GetSpellInfo(88423)),members[i].Unit)
						--PQR_WriteToChat("\124cFFFF55FFLeft alt key down - Auto dispeled!")
						return true
					end
				end
			end
		end
	end
end
--------------------------------------------------------------------
--NaturesCure Mouseover Природный целитель
if PQR_SpellAvailable(88423)
and select(2,GetSpellCooldown(88423)) < 2
and UnitExists("mouseover")
and IsUsableSpell(88423)
and IsSpellInRange(GetSpellInfo(88423),"mouseover") == 1
then
	if ValidDispel("mouseover")
	and UnitIsFriend("player","mouseover")
	and CanHeal("mouseover")
	and not UnitCastingInfo("player") then
		--PQR_WriteToChat("\124cFFFF55FFMouseover - Auto dispeled!")
		CastSpellByName(tostring(GetSpellInfo(88423)),"mouseover")
		return true
	end
end
--------------------------------------------------------------------
--NatureSwiftness Природная стремительность
if Nova_VNatureSwiftnessCheck then
	if not UnitBuffID("player",132158)
	and PQR_SpellAvailable(132158)
	and select(2,GetTalentRowSelectionInfo(2)) == 4
	and select(2,GetSpellCooldown(132158)) < 2
	then
		if members[1].HP <= Nova_VNatureSwiftness
		and not UnitCastingInfo("player") then
			CastSpellByName(tostring(GetSpellInfo(132158)),"player")
			return true
		end
	end
end

----------------------------------------------------------------------------
-- Nourish Покровительство Природы
if Nova_VNourishCheck then
	if PQR_SpellAvailable(50464)
	 --and UnitAffectingCombat("player")
	 and IsUsableSpell(50464)
	 and not Nova_Moving
	 and (UnitBuffID(members[1].Unit, 774, "PLAYER") --Rejuvenation
	 or UnitBuffID(members[1].Unit, 8936, "PLAYER") --Regrowth
	 or UnitBuffID(members[1].Unit, 33763, "PLAYER") --Lifebloom
	 or UnitBuffID(members[1].Unit, 48438, "PLAYER")) --Wild Growth
	 and CanHeal(members[1].Unit)
	 and IsSpellInRange(GetSpellInfo(50464),members[1].Unit) == 1
	 and not stopcasting then
		if members[1].HP <= Nova_VNourish
		and lowhpmembers < 3
		and not UnitCastingInfo("player") then
			--PQR_CustomTarget = members[1].Unit
			CastSpellByName(tostring(GetSpellInfo(50464)),members[1].Unit)
			--PQR_WriteToChat("\124cFFFF55FFCasting Nourish - HP: " ..members[1].HP)
			return true
		elseif not UnitBuffID("player", 100977) --Harmony 77495
		and (members[1].HP < (Nova_VNourish+10))
		and not UnitCastingInfo("player") then
			--PQR_CustomTarget = members[1].Unit
			CastSpellByName(tostring(GetSpellInfo(50464)),members[1].Unit)
			--PQR_WriteToChat("\124cFFFF55FFCasting Nourish for Harmony - HP: " ..members[1].HP)
			return true
		elseif UnitBuffID("player", 100977) --Harmony 77495
		and (members[1].HP < (Nova_VNourish+10)) then
			if (select(7, UnitBuffID("player", 100977)) - GetTime() <= 2)
			and not UnitCastingInfo("player") then
				--PQR_CustomTarget = members[1].Unit
				CastSpellByName(tostring(GetSpellInfo(50464)),members[1].Unit)
				--PQR_WriteToChat("\124cFFFF55FFCasting Nourish for Harmony - HP: " ..members[1].HP)
				return true
			end
		end
	end
end
----------------------------------------------------------------------------
--Regrowth - duration automatically refreshes to 6 sec each time Regrowth heals targets at or below 50% health.
--ToL support Восстановление
if Nova_VRegrowthCheck then
	if PQR_SpellAvailable(8936)
	 --and UnitAffectingCombat("player")
	 and IsSpellInRange(GetSpellInfo(8936),members[1].Unit) == 1
	 and CanHeal(members[1].Unit)
	 and IsUsableSpell(8936)
	 --and not UnitBuffID(members[1].Unit, 48504, "PLAYER") --Living Seed
	 and not Nova_Moving
	 and not stopcasting then
		if members[1].HP < Nova_VRegrowth and select(3, Nova_UnitInfo("player")) > 20
		and not UnitCastingInfo("player") then
			--NatureSwiftness
			if not UnitBuffID("player",132158)
			and PQR_SpellAvailable(132158)
			and select(2,GetTalentRowSelectionInfo(2)) == 4
			and select(2,GetSpellCooldown(132158)) < 2
			and not UnitCastingInfo("player")
			then
				CastSpellByName(tostring(GetSpellInfo(132158)),"player")
				--PQR_WriteToChat("\124cFFFF55FFCast NatureSwiftness. HP: "..members[1].HP)
			end
			CastSpellByName(tostring(GetSpellInfo(8936)),members[1].Unit)
		 	--PQR_CustomTarget = members[1].Unit
		 	return true
		elseif UnitBuffID("player", 16870)
		and Nova_CheckLastCast(8936, 0.5)
		and members[1].HP <= Nova_VOmenRegrowth
		and not UnitCastingInfo("player") then --Omen of Clarity buff cost no mana
			CastSpellByName(tostring(GetSpellInfo(8936)),members[1].Unit)
		 	--PQR_CustomTarget = members[1].Unit
		 	--PQR_WriteToChat("\124cFFFF55FFCast OmenRegrowth. HP: "..members[1].HP)
		 	return true
		end
	end
end

-----------------------------------------------------------------------
--RejuSwiftmendSmart Омоложение
if Nova_VSwiftmendCheck then
	--if Nova_VSmartCheck then
		--if UnitAffectingCombat("player")
		if not RejuSM then
			RejuSM = { }
		end

		if PQR_SpellAvailable(774) and IsUsableSpell(774) and select(2,GetSpellCooldown(774)) < 2 then
			if #RejuSM < 1 then
				if AverageHealth(Nova_VSwiftmendLimit) <= Nova_VSwiftmend then
					PRGetDistanceTable((Nova_VSwiftmend+10), 10, (Nova_VSwiftmendLimit-1))
					PRGetDistanceTablebyHP(Nova_VSwiftmend,(Nova_VSwiftmendLimit-1))
					if IsSpellInRange(GetSpellInfo(774),prdistancebyhp[1].Unit)
					and CanHeal(prdistancebyhp[1].Unit)
					and (prdistancebyhp[1].PD >= (Nova_VSwiftmendLimit-1))
					and prdistancebyhp[1].HP <= Nova_VSwiftmend
					and not UnitCastingInfo("player") then
						if UnitBuffID(prdistancebyhp[1].Unit, 8936)
						and not UnitBuffID(prdistancebyhp[1].Unit, 774)
						and not UnitCastingInfo("player") then
							table.insert(RejuSM, { RSMUnit = prdistancebyhp[1].Unit, RSMCastTime = GetTime(), RSMHP = prdistancebyhp[1].HP, RSMPD = prdistancebyhp[1].PD  } )
							CastSpellByName(tostring(GetSpellInfo(774)),prdistancebyhp[1].Unit)
							--PQR_WriteToChat("\124cFFFF55FFCast Reju then SM - Name: " ..UnitName(prdistancebyhp[1].Unit).." - HP: "..prdistancebyhp[1].HP.." - total around: "..prdistancebyhp[1].PD)
							return true
						else
							table.insert(RejuSM, { RSMUnit = prdistancebyhp[1].Unit, RSMCastTime = GetTime(), RSMHP = prdistancebyhp[1].HP, RSMPD = prdistancebyhp[1].PD  } )
							--PQR_WriteToChat("\124cFFFF55FFInsert member to SM - Name: " ..UnitName(prdistancebyhp[1].Unit).." - HP: "..prdistancebyhp[1].HP.." - total around: "..prdistancebyhp[1].PD)
							return true
						end
					end
				end
			else
				if Nova_UnitInfo(RejuSM[1].RSMUnit) > 95 and ((GetTime() - RejuSM[1].RSMCastTime) >= 3) then
					--PQR_WriteToChat("\124cFFFF55FFClear RejuSM 1")
					table.wipe(RejuSM)
					RejuSM = { }
					return true
				end

				if not UnitBuffID(RejuSM[1].RSMUnit, 8936)
				and not UnitBuffID(RejuSM[1].RSMUnit, 774)
				and IsSpellInRange(GetSpellInfo(774),RejuSM[1].RSMUnit)
				and CanHeal(RejuSM[1].RSMUnit)
				and not UnitCastingInfo("player") then
					CastSpellByName(tostring(GetSpellInfo(774)),RejuSM[1].RSMUnit)
					--PQR_WriteToChat("\124cFFFF55FFRenew Reju then SM - Name: " ..UnitName(RejuSM[1].RSMUnit))
					return true
				end
			end
		end
	--end
end
--------------------------------------------------------------------
-- Rejuvenation
if Nova_VRejuvenationTankCheck then
	if PQR_SpellAvailable(774)
	and IsUsableSpell(774)
	and Nova_CheckLastCast(774, 2.0) then
	--and UnitAffectingCombat("player") then
		if #Nova_Tanks > 0 then
			for i=1, #Nova_Tanks do
				if UnitThreatSituation(Nova_Tanks[i].Unit)
				 and select(3, Nova_UnitInfo("player")) > 15
				 and Nova_Tanks[i].HP <= Nova_VRejuvenationTank
				 and CanHeal(Nova_Tanks[i].Unit)
				 and not UnitCastingInfo("player")
				 and IsSpellInRange(GetSpellInfo(774),Nova_Tanks[i].Unit) == 1 then
					if UnitBuffID(Nova_Tanks[i].Unit, 774, "PLAYER") then
						if (select(7, UnitBuffID(Nova_Tanks[i].Unit, 774, "PLAYER")) - GetTime() <= 2)
						and not UnitCastingInfo("player") then
							--PQR_CustomTarget = Nova_Tanks[i].Unit
							CastSpellByName(tostring(GetSpellInfo(774)),Nova_Tanks[i].Unit)
							--PQR_WriteToChat("\124cFFFF55FFRejuvenation soon expired on tank!")
							return true
						end
					else
						--PQR_CustomTarget = Nova_Tanks[i].Unit
						CastSpellByName(tostring(GetSpellInfo(774)),Nova_Tanks[i].Unit)
						--PQR_WriteToChat("\124cFFFF55FFRejuvenation tank!")
						return true
					end
				end
			end
		end

		--if UnitThreatSituation(members[1].Unit)	== 3
		if select(3, Nova_UnitInfo("player")) > 20
		--and not UnitBuffID(members[1].Unit, 774, "PLAYER")
		and not UnitBuffID(members[1].Unit, 774)
		and members[1].HP < Nova_VRejuvenation
		and CanHeal(members[1].Unit)
		and IsSpellInRange(GetSpellInfo(774),members[1].Unit) == 1
		and not UnitCastingInfo("player") then
		 	--PQR_CustomTarget = members[1].Unit
		 	CastSpellByName(tostring(GetSpellInfo(774)),members[1].Unit)
		 	--PQR_WriteToChat("\124cFFFF55FFRejuvenation high threat member!")
		 	return true
		end

		--for i=1, #members do
			--if not UnitBuffID(members[i].Unit, 774)
			 --and members[i].HP < Nova_Rejuvenation
			 --and select(3, Nova_UnitInfo("player")) > 20
			 --and CanHeal(members[i].Unit)
		     --and IsSpellInRange(GetSpellInfo(774),members[i].Unit) == 1 then
			 	--PQR_CustomTarget = members[i].Unit
			 	--PQR_WriteToChat("\124cFFFF55FFRejuvenation member!")
			 	--return true
			--end
		--end
	end
end

--------------------------------------------------------------------------------
-- SpiritWalkerGrace Благосклонность предков
--Symn SpiritWalker's Grace from Shaman (work good with Tranquility) or Cleanse from Pally
if Nova_VSpiritWalkerGraceCheck then
	if members[1].HP < Nova_VSpiritWalkerGrace
	and UnitAffectingCombat("player")
	and IsUsableSpell(110806) --druid spell id 79206
	and PQR_IsMoving(1)
	and not UnitCastingInfo("player")
	and PQR_SpellAvailable(110806)
	then
		CastSpellByName(tostring(GetSpellInfo(110806),nil))
		--PQR_WriteToChat("\124cFFFF55FFSpirit walker grace - HP: "..members[1].HP)
		return true
	end
end

---------------------------------------------------------------------------------------------
--SwiftmendSmart Быстрое восстановление
if Nova_VSwiftmendCheck
and not LLdebuff then
	--if Nova_VSmartCheck then
		--if UnitAffectingCombat("player")
		--and PQR_SpellAvailable(18562)
		--and IsUsableSpell(18562)
		if #RejuSM > 0 then
			if GetSpellCooldown(18562) == 0 then
				if UnitBuffID(RejuSM[1].RSMUnit, 8936)
				or UnitBuffID(RejuSM[1].RSMUnit, 774) then
					if IsSpellInRange(GetSpellInfo(18562),RejuSM[1].RSMUnit)
					and CanHeal(RejuSM[1].RSMUnit) then
						if (UnitCastingInfo("player") == GetSpellInfo(50464)) --Nourish
						then
							SpellStopCasting()
							--PQR_WriteToChat("\124cFFFF55FFStopCast Nourish for RejuSM ")
						end
						CastSpellByName(tostring(GetSpellInfo(18562)),RejuSM[1].RSMUnit)
						--PQR_WriteToChat("\124cFFFF55FFCast SwiftmendSmart - Name: " ..UnitName(RejuSM[1].RSMUnit))
						return true
					end
				end
			end

			if ((GetTime() - RejuSM[1].RSMCastTime) >= 7) then
				--PQR_WriteToChat("\124cFFFF55FFClear RejuSM 2")
				table.wipe(RejuSM)
				RejuSM = { }
				return true
			end
		end
	--end
end
---------------------------------------------------------------------------------

--WildGrowth - ToL support
--if Nova_VSmartCheck then Буйный рост
	if Nova_VWildGrowthCheck then
		if PQR_SpellAvailable(48438)
		and IsUsableSpell(48438)
		and select(2,GetSpellCooldown(48438)) < 2
		and not LLdebuff then
		--and UnitAffectingCombat("player") then
			--if select(3, Nova_UnitInfo("player")) >= Nova_LowMana
			--and members[1].HP <= Nova_WildGrowth
			--and lowhpmembers >= Nova_WildGrowthLimit then
			if AverageHealth(Nova_VWildGrowthLimit) <= Nova_VWildGrowth then
				PRGetDistanceTable((Nova_VWildGrowth+5), 30, (Nova_VWildGrowthLimit-1))
				PRGetDistanceTablebyHP(Nova_VWildGrowth,(Nova_VWildGrowthLimit-1))

				if IsSpellInRange(GetSpellInfo(48438),prdistancebyhp[1].Unit)
				and CanHeal(prdistancebyhp[1].Unit)
				and (prdistancebyhp[1].PD >= (Nova_VWildGrowthLimit-1))
				and prdistancebyhp[1].HP <= Nova_VWildGrowth
				and not UnitCastingInfo("player") then
					CastSpellByName(tostring(GetSpellInfo(48438)),prdistancebyhp[1].Unit)
					--PQR_CustomTarget = prdistancebyhp[1].Unit
					--PQR_WriteToChat("\124cFFFF55FFCast WildGrowth - Name: " ..UnitName(prdistancebyhp[1].Unit).." - HP: "..prdistancebyhp[1].HP.." - PD: "..prdistancebyhp[1].PD)
					return true
				end

			end
		end
	end
--end
---------------------------------------------------------------------------