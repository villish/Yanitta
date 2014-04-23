-- Проверяем доступность заклинания.
function IsPlayerKnownSpell(spellName)
    -- Основная проверка
    if IsPlayerSpell(ability.SpellId)
        or IsSpellKnown(ability.SpellId) or IsSpellKnown(ability.SpellId, true)
        or GetSpellBookItemInfo(spellInfo) then
        return true;
    end
    -- Проверка на доступность заклинания по уровню
    if IsSpellClassOrSpec(spellName) and UnitLevel("player") >= GetSpellAvailableLevel(ability.SpellId) then
        -- Проверка на доступность заклинания таланта
        IsTalentSpell()
        GetSpecializationSpells()
        IsSpellClassOrSpec()
    end
    
    if IsTalentSpell(spellName) then
        for row = 1, GetNumTalentTabs() do
            local ... = GetTalentInfo()
        end
    end
end