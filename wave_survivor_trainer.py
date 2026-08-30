import os
import json
import struct
import pymem
import pymem.process

PROCESS_NAME = "WaveSurvivor.exe"
SAVE_FILE_NAME = "wave_survivor_save.json"

# ==========================================
# 1. SAVE FILE SYSTEM (PERSISTENT MODS)
# ==========================================

print("""
Capabilities & Memory Mapping Summary:
1) Save File Editor (Option 1): 
- Scans %USERPROFILE%\\AppData\\LocalLow for wave_survivor_save.json. Rewrites gold and stage clear requirements directly in the save dictionary

2) Live Player HP Control (Option 3): 
- Scans RAM for the integer currentHealth stored in PlayerHealth.cs and overwrites it to grant god-mode or custom health.

3) Live Level & XP Control (Option 4): Reads CurrentExperience and Level from PlayerStats.cs and forces immediate level ups.

4) Attack & Speed Multipliers (Options 5 & 6): 
- Targets AtkPercentMultiplier and SpeedPercentMultiplier in PlayerStats.cs. Pushing 5.0 increases player damage output by $+500\\%$

5) Enemy Speed & Damage Modifiers (Options 7, 8, & 9): 
- Scans for speedMultiplier, currentHealth, and contactDamage in EnemyController.cs and EnemyHealth.cs to freeze monsters, set their health to 1, or remove contact damage.  
""")


def find_save_file():
    """Locates wave_survivor_save.json in AppData/LocalLow."""
    user_profile = os.environ.get("USERPROFILE", "")
    locallow = os.path.join(user_profile, "AppData", "LocalLow")
    
    if os.path.exists(locallow):
        for root, dirs, files in os.walk(locallow):
            if SAVE_FILE_NAME in files:
                return os.path.join(root, SAVE_FILE_NAME)
    return None

def modify_save_file(gold=999999, unlock_all=True, potion_count=99):
    """Edits persistent progress data directly on disk."""
    save_path = find_save_file()
    if not save_path or not os.path.exists(save_path):
        print(f"[-] Save file '{SAVE_FILE_NAME}' not found. Run the game once to create a save.")
        return

    try:
        with open(save_path, "r") as f:
            data = json.load(f)

        # Update Gold & Progression
        data["gold"] = max(0, gold)
        if unlock_all:
            data["highestUnlockedStage"] = 5
            for stage in data.get("stages", []):
                stage["completed"] = True
                stage["bossDefeated"] = True

        with open(save_path, "w") as f:
            json.dump(data, f, indent=4)

        print(f"[+] Save modified successfully! Path: {save_path}")
        print(f"    -> Gold set to: {gold}")
        print(f"    -> Highest Stage Unlocked: {data['highestUnlockedStage']}")
    except Exception as e:
        print(f"[-] Failed to update save file: {e}")


# ==========================================
# 2. PYMEM LIVE MEMORY CONTROLLER
# ==========================================
class MemoryTrainer:
    def __init__(self):
        self.pm = None

    def attach(self):
        """Attaches Pymem to the running game process."""
        try:
            self.pm = pymem.Pymem(PROCESS_NAME)
            print(f"[+] Attached to {PROCESS_NAME} (PID: {self.pm.process_id})")
            return True
        except Exception:
            print(f"[-] {PROCESS_NAME} process not found. Please launch the game first.")
            return False

    def write_int_pattern(self, current_val, new_val, max_matches=10):
        """Scans process memory for target int and updates it."""
        if not self.pm:
            return 0
        pattern = current_val.to_bytes(4, byteorder='little', signed=True)
        matches = self.pm.pattern_scan_all(pattern)
        count = 0
        if matches:
            for addr in matches[:max_matches]:
                try:
                    self.pm.write_int(addr, new_val)
                    count += 1
                except Exception:
                    continue
        return count

    def write_float_pattern(self, current_val, new_val, max_matches=10):
        """Scans process memory for target float and updates it."""
        if not self.pm:
            return 0
        pattern = struct.pack('f', current_val)
        matches = self.pm.pattern_scan_all(pattern)
        count = 0
        if matches:
            for addr in matches[:max_matches]:
                try:
                    self.pm.write_float(addr, new_val)
                    count += 1
                except Exception:
                    continue
        return count


# ==========================================
# 3. INTERACTIVE CLI INTERFACE
# ==========================================
def main():
    trainer = MemoryTrainer()

    while True:
        print("\n" + "=" * 45)
        print("    WAVE SURVIVOR MASTER CHEAT TRAINER    ")
        print("=" * 45)
        print("--- [PERSISTENT SAVE MODS (OFFLINE/ONLINE)] ---")
        print("[1] Set Custom Gold & Unlock All 5 Stages")
        print("\n--- [LIVE GAME MEMORY MODS (PYMEM)] ---")
        print("[2] Attach to WaveSurvivor.exe")
        print("[3] Player: Modify Live Health (Set HP / God Mode)")
        print("[4] Player: Modify Live XP & Level")
        print("[5] Player: Set Attack Boost Multiplier (+30% to +1000%)")
        print("[6] Player: Set Movement Speed Multiplier (+50% to +1000%)")
        print("[7] Enemies: Freeze / Slow All Enemy Movement")
        print("[8] Enemies: Weakness Curse (One-Shot Enemy HP)")
        print("[9] Enemies: Set Contact Damage to Zero")
        print("[0] Exit Trainer")
        print("=" * 45)

        choice = input("Select an option: ").strip()

        if choice == "1":
            gold_in = int(input("Enter total Gold desired (e.g., 99999): "))
            modify_save_file(gold=gold_in, unlock_all=True)

        elif choice == "2":
            trainer.attach()

        elif choice == "3":
            if not trainer.pm and not trainer.attach(): continue
            cur_hp = int(input("Enter your CURRENT displayed HP in game: "))
            new_hp = int(input("Enter desired HP (e.g., 9999): "))
            changed = trainer.write_int_pattern(cur_hp, new_hp)
            print(f"[+] Updated {changed} health entries in memory.")

        elif choice == "4":
            if not trainer.pm and not trainer.attach(): continue
            cur_xp = int(input("Enter your CURRENT displayed XP in game: "))
            new_xp = int(input("Enter desired XP amount: "))
            changed = trainer.write_int_pattern(cur_xp, new_xp)
            print(f"[+] Updated {changed} XP entries in memory.")

        elif choice == "5":
            if not trainer.pm and not trainer.attach(): continue
            print("Base AtkMultiplier: Default is 0.0 (or 0.30 if potion active).")
            mult = float(input("Enter desired multiplier (e.g., 5.0 for +500% ATK): "))
            changed = trainer.write_float_pattern(0.0, mult)
            if changed == 0:
                changed = trainer.write_float_pattern(0.30, mult)
            print(f"[+] Updated {changed} Attack Multiplier fields in memory.")

        elif choice == "6":
            if not trainer.pm and not trainer.attach(): continue
            print("Base SpeedMultiplier: Default is 0.0 (or 0.50 if potion active).")
            mult = float(input("Enter desired speed multiplier (e.g., 3.0 for +300% Speed): "))
            changed = trainer.write_float_pattern(0.0, mult)
            if changed == 0:
                changed = trainer.write_float_pattern(0.50, mult)
            print(f"[+] Updated {changed} Speed Multiplier fields in memory.")

        elif choice == "7":
            if not trainer.pm and not trainer.attach(): continue
            print("Enemy Speed Multiplier target: Default is 1.0.")
            mult = float(input("Enter multiplier (0.0 to Freeze, 0.2 for Super Slow): "))
            changed = trainer.write_float_pattern(1.0, mult)
            print(f"[+] Modified speed for {changed} enemy instances.")

        elif choice == "8":
            if not trainer.pm and not trainer.attach(): continue
            cur_e_hp = int(input("Enter displayed Enemy Health (e.g., 10 or 3): "))
            changed = trainer.write_int_pattern(cur_e_hp, 1)
            print(f"[+] Reduced HP to 1 for {changed} active enemies.")

        elif choice == "9":
            if not trainer.pm and not trainer.attach(): continue
            cur_dmg = int(input("Enter enemy contact damage (e.g., 1 or 2): "))
            changed = trainer.write_int_pattern(cur_dmg, 0)
            print(f"[+] Set contact damage to 0 for {changed} enemy instances.")

        elif choice == "0":
            print("Exiting Trainer.")
            break

if __name__ == "__main__":
    main()