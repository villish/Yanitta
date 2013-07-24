        ============================================
        =========    Справка по Yanitta    =========
        ============================================

  ***  LUA  ***
Немножко теории, для написания условий по использованию заклинаний в ротации необходимо придерживатся минимализма.
Добалять отладочные сообщения в условие (для отладки и переходе на новые патчи игрового клиента)
Использовать неявное приведение типов (пример: if test_var then ... end), если переменная == nil тогда ее проверять на nil ненадо.

Для каждого класса есть свой модуль глобальных функций, туда лучше складывать собственные функции и переменные (метатаблицы).

  ***  Ротация  ***
Ротация представляет собой приоритетность использования заклинаний.
Выполнимый луа код в модуле болжен возвращать true если заклинание должно выполнится (если не надо чтобы выполнялось условие то достаточно вернуть false или nil
Пример возврата nil

if (условие не выполняем) then
    return true
end

в данном случае получим nil или же false

  ***  Правила  ***
Исходя из того, что во время выполнения боевой ротации необходимо выполнять какие-то действия (вне ротации) например прожать кулдауны, 
использовать особое закринание или просто прекратить наносить урон, необходимо строго зарезервировать нажатие на клавиши модификаторы
(LeftControl, RightControl, LeftShift, RightShift, LeftAlt, RightAlt).
Я считаю, что на левую строну лучше забиндить то, что используется часто:
Например:
    LeftControl				- Включение/выключение AOE/SingleTarget ротации
    LeftShift				- Особое действие под mouseover (для воина героический прыжек, для охотника бросок ловушки)
    LeftAlt					- Пауза в ротации

А на правую, то что используется реже (или не так активно)
Например:
    RightControl			- Включение/выключение кулдаунов
    RightShift				- Дополнительное особое действие (тот же бросок ловушки, но другой, к примеру на левом - огненной, а тут ледяной)
    RightAlt				- пока хз... на свое усмотрение

Описание функций:
    IsModifierKeyDown       - Returns whether a modifier key is held down

    IsShiftKeyDown          - Returns whether a Shift key on the keyboard is held down
    IsAltKeyDown            - Returns whether a Alt key on the keyboard is held down
    IsControlKeyDown        - Returns whether a Control key on the keyboard is held down

    IsLeftAltKeyDown        - Returns whether the left Alt key is currently held down
    IsLeftControlKeyDown    - Returns whether the left Control key is held down
    IsLeftShiftKeyDown      - Returns whether the left Shift key on the keyboard is held down

    IsRightAltKeyDown       - Returns whether the right Alt key is currently held down
    IsRightControlKeyDown   - Returns whether the right Control key on the keyboard is held down
    IsRightShiftKeyDown     - Returns whether the right shift key on the keyboard is held down

    ***
продолжение следует... (а может и нет)
