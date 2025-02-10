# Eitr Magic Extended

Control various eitr aspects. Linear regeneration rate change, more regen from extra eitr or food, extra base eitr from skills. Magic shield tweaks.

## Linear regeneration rate change

Makes eitr recovery rate dependent on the current value. Overall time to regenerate eitr to 100% is still almost the same.

If Multiplier value is above 1. Eitr will regenerate faster at lower values and proportionally slower at higher values. 

If Multiplier value is below 1. Eitr will regenerate slower at lower values and proportionally higher at higher values.

You can set regeneration threshold aka inflection point of eitr regeneration rate. In that point regeneration rate changes its sign.

For example using default values (multiplier 3.0, threshold 0.5):
* eitr 0% - regeneration rate is 300% of normal
* eitr 25% - regeneration rate is 150% of normal
* eitr 50% - regeneration rate is 100% of normal
* eitr 75% - regeneration rate is 66% of normal
* eitr 100% - regeneration rate is 33% of normal

## Extra eitr regeneration

Makes extra eitr points increase overall regeneration rate.

Default config is 1% of regeneration rate per every 5 points of extra eitr (gained from any source like food, enchants, jewels or any other source of eitr increase outside of base eitr).

You can limit the calculation of extra eitr so it takes into account eitr from food only.

Default config of 1% regeneration rate per 5 extra points means you will have 50% eitr regeneration rate if you have 250 eitr from your food.
That way you can still use Minor eitr mead instead of Lingering eitr mead.

Regeneration rate will be shown in the food tooltip and in the Raven menu in Status effects section.

## Base eitr increase by skills

Base eitr will be increased proportionally by skills levels where max value reached at skill level 100.

Skills that affect base eitr:
* Elemental magic
* Blood magic

Value is configurable skill-wise.

Default config means you will have 20 more base eitr for any skill with level 50 and 40 eitr for skill level 100.
So maximum increase will be 80 eitr when both skills are 100.

There is alternative way of gaining base eitr - non linear formula.

This way you will have more eitr at low skill levels and less eitr on high skill level.

Default formula is 3 * (skill ^ 0.5), 3 multiplied by skill factor raised to the power of 0.5 (square root).

## Misc settings

Hide eitr numeric value shown on the bar.

## Shield tweaks

Tired of not be able to properly update your shield when you are up to face a new danger? Or nervous when you can't say if your shield is about to break or not?

There is a config options for that now:
* add a secondary attack to Staff of Protection which will break current shield (without skill raise obviously)
* shield color will gradually change color the less hp it can absorb (color is configurable)

And a couple of other configs to prevent:
* hit effect spam. Sometimes other mods remove damage from incoming hits without removing hits and shield still plays hit effect even without taking damage. Now if hit has 0 damage hit effect will not be played.
* console and log file "Look Rotation Viewing Vector Is Zero" spam when shield takes indirect hit

And finally an option for shield to fully prevent different configurable types of damage like falling, drowning, burning, poison and others. This feels cheating but that's up to you.

## Installation (manual)
extract EitrMagicExtended.dll to your BepInEx\Plugins\ folder

## Configurating
The best way to handle configs is [Configuration Manager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/).

Or [Official BepInEx Configuration Manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/).

## Mirrors
[Nexus](https://www.nexusmods.com/valheim/mods/2961)

## Donation
[Buy Me a Coffee](https://buymeacoffee.com/shudnal)