local unit = "target";

for tabIndex = 1, GetNumSpellTabs() do
    local name, texture, offset, numSpells = GetSpellTabInfo(tabIndex);
    for spellIndex = offset, numSpells do
		local skillType, spellId = GetSpellBookItemInfo(spellIndex, "spell");
        if UnitExists(unit) and spellId then
			local spellName = GetSpellInfo(spellId) or '';
			local isRange = nil;
			if spellName then
				isRange = IsSpellInRange(spellName, unit);
			end
			if isRange then	
				print("|cff00ff00id: "..spellId.." name: "..spellName.." - "..isRange);
			else
				print("|cffff0000id: "..spellId.." name: "..spellName.." - <error>");	
			end			
		else
			print("|cffff0000Can't target");
		end
    end
end