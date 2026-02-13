using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
public class WeaponsSOAssigner : EditorWindow
{

    public string weaponsRootPath = "Assets/MainGame/Player/Prefabs/Weapons"; // Root folder containing all weapon folders
    List<WeaponData> weapons = new List<WeaponData> {
    new WeaponData { name="AK-47", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=35f, bodyDamage=25f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=600, magSize=30, pelletCount=1, weaponPrefab_rb="AK-47_RB", weaponPrefab_onplayer="AK-47_OnPlayer" },
    new WeaponData { name="Colt_M4A1", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=34f, bodyDamage=24f, legDamage=14f, isThrowable=false, hasAreaEffect=false, fireRate=700, magSize=30, pelletCount=1, weaponPrefab_rb="M4A1_RB", weaponPrefab_onplayer="M4A1_OnPlayer" },
    new WeaponData { name="FN_SCAR_L", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=38f, bodyDamage=27f, legDamage=17f, isThrowable=false, hasAreaEffect=false, fireRate=600, magSize=30, pelletCount=1, weaponPrefab_rb="SCAR_L_RB", weaponPrefab_onplayer="SCAR_L_OnPlayer" },
    new WeaponData { name="HK_416", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=36f, bodyDamage=26f, legDamage=16f, isThrowable=false, hasAreaEffect=false, fireRate=750, magSize=30, pelletCount=1, weaponPrefab_rb="HK416_RB", weaponPrefab_onplayer="HK416_OnPlayer" },
    new WeaponData { name="VSS_Vintorez", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=50f, bodyDamage=35f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=300, magSize=10, pelletCount=1, weaponPrefab_rb="VSS_RB", weaponPrefab_onplayer="VSS_OnPlayer" },
    new WeaponData { name="Barret_M82A1", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=150f, bodyDamage=100f, legDamage=50f, isThrowable=false, hasAreaEffect=false, fireRate=50, magSize=5, pelletCount=1, weaponPrefab_rb="Barret_RB", weaponPrefab_onplayer="Barret_OnPlayer" },
    new WeaponData { name="HK_G3", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=40f, bodyDamage=28f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=550, magSize=20, pelletCount=1, weaponPrefab_rb="G3_RB", weaponPrefab_onplayer="G3_OnPlayer" },
    new WeaponData { name="HK33", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=32f, bodyDamage=22f, legDamage=14f, isThrowable=false, hasAreaEffect=false, fireRate=750, magSize=30, pelletCount=1, weaponPrefab_rb="HK33_RB", weaponPrefab_onplayer="HK33_OnPlayer" },
    new WeaponData { name="HK417A2", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=45f, bodyDamage=30f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=600, magSize=20, pelletCount=1, weaponPrefab_rb="HK417_RB", weaponPrefab_onplayer="HK417_OnPlayer" },
    new WeaponData { name="Armalite_AR10", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=40f, bodyDamage=28f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=550, magSize=20, pelletCount=1, weaponPrefab_rb="AR10_RB", weaponPrefab_onplayer="AR10_OnPlayer" },
    new WeaponData { name="Colt_M16A2", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=34f, bodyDamage=24f, legDamage=14f, isThrowable=false, hasAreaEffect=false, fireRate=700, magSize=30, pelletCount=1, weaponPrefab_rb="M16A2_RB", weaponPrefab_onplayer="M16A2_OnPlayer" },
    new WeaponData { name="HK_G28", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=45f, bodyDamage=30f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=500, magSize=20, pelletCount=1, weaponPrefab_rb="G28_RB", weaponPrefab_onplayer="G28_OnPlayer" },
    new WeaponData { name="MK12", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=42f, bodyDamage=30f, legDamage=16f, isThrowable=false, hasAreaEffect=false, fireRate=600, magSize=20, pelletCount=1, weaponPrefab_rb="MK12_RB", weaponPrefab_onplayer="MK12_OnPlayer" },
    new WeaponData { name="MK17", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=50f, bodyDamage=35f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=450, magSize=20, pelletCount=1, weaponPrefab_rb="MK17_RB", weaponPrefab_onplayer="MK17_OnPlayer" },
    new WeaponData { name="SR_25_Apply", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=85f, bodyDamage=65f, legDamage=40f, isThrowable=false, hasAreaEffect=false, fireRate=200, magSize=10, pelletCount=1, weaponPrefab_rb="SR25_RB", weaponPrefab_onplayer="SR25_OnPlayer" },
    new WeaponData { name="Remington_Scoplu_Applied", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=70f, bodyDamage=50f, legDamage=35f, isThrowable=false, hasAreaEffect=false, fireRate=150, magSize=5, pelletCount=1, weaponPrefab_rb="RemingtonScope_RB", weaponPrefab_onplayer="RemingtonScope_OnPlayer" },
    new WeaponData { name="HK_MP5", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=30f, bodyDamage=20f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=800, magSize=30, pelletCount=1, weaponPrefab_rb="MP5_RB", weaponPrefab_onplayer="MP5_OnPlayer" },
    new WeaponData { name="Sig_Saguer_Mpx", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=28f, bodyDamage=18f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=850, magSize=30, pelletCount=1, weaponPrefab_rb="SigMPX_RB", weaponPrefab_onplayer="SigMPX_OnPlayer" },
    new WeaponData { name="ARP-9", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=27f, bodyDamage=17f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=900, magSize=30, pelletCount=1, weaponPrefab_rb="ARP9_RB", weaponPrefab_onplayer="ARP9_OnPlayer" },
    new WeaponData { name="KrissVector", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=28f, bodyDamage=18f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=950, magSize=25, pelletCount=1, weaponPrefab_rb="Kriss_RB", weaponPrefab_onplayer="Kriss_OnPlayer" },
    new WeaponData { name="UMP45", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=32f, bodyDamage=22f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=750, magSize=25, pelletCount=1, weaponPrefab_rb="UMP45_RB", weaponPrefab_onplayer="UMP45_OnPlayer" },
    new WeaponData { name="SIG556", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=33f, bodyDamage=23f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=700, magSize=30, pelletCount=1, weaponPrefab_rb="SIG556_RB", weaponPrefab_onplayer="SIG556_OnPlayer" },
    new WeaponData { name="CZ_Scorpion_Evo", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=28f, bodyDamage=18f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=850, magSize=20, pelletCount=1, weaponPrefab_rb="CZ_Scorpion_RB", weaponPrefab_onplayer="CZ_Scorpion_OnPlayer" },
    new WeaponData { name="G36C", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=35f, bodyDamage=25f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=750, magSize=30, pelletCount=1, weaponPrefab_rb="G36C_RB", weaponPrefab_onplayer="G36C_OnPlayer" },
    new WeaponData { name="Mossberg_500_SWAT", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=60f, bodyDamage=45f, legDamage=25f, isThrowable=false, hasAreaEffect=true, fireRate=80, magSize=8, pelletCount=8, weaponPrefab_rb="Mossberg_RB", weaponPrefab_onplayer="Mossberg_OnPlayer" },
    new WeaponData { name="Kel_Tec_KS7", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=55f, bodyDamage=40f, legDamage=20f, isThrowable=false, hasAreaEffect=true, fireRate=90, magSize=7, pelletCount=9, weaponPrefab_rb="Keltec_RB", weaponPrefab_onplayer="Keltec_OnPlayer" },
    new WeaponData { name="Benelli_M4", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=65f, bodyDamage=50f, legDamage=30f, isThrowable=false, hasAreaEffect=true, fireRate=90, magSize=7, pelletCount=8, weaponPrefab_rb="Benelli_RB", weaponPrefab_onplayer="Benelli_OnPlayer" },
    new WeaponData { name="AA12", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=50f, bodyDamage=40f, legDamage=25f, isThrowable=false, hasAreaEffect=true, fireRate=300, magSize=20, pelletCount=10, weaponPrefab_rb="AA12_RB", weaponPrefab_onplayer="AA12_OnPlayer" },
    new WeaponData { name="Glock_17", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=25f, bodyDamage=15f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=400, magSize=17, pelletCount=1, weaponPrefab_rb="Glock17_RB", weaponPrefab_onplayer="Glock17_OnPlayer" },
    new WeaponData { name="Desert_Eagle", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=50f, bodyDamage=35f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=300, magSize=7, pelletCount=1, weaponPrefab_rb="DesertEagle_RB", weaponPrefab_onplayer="DesertEagle_OnPlayer" },
    new WeaponData { name="Makarov", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=22f, bodyDamage=14f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=350, magSize=8, pelletCount=1, weaponPrefab_rb="Makarov_RB", weaponPrefab_onplayer="Makarov_OnPlayer" },
    new WeaponData { name="Luger_P08", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=25f, bodyDamage=15f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=350, magSize=8, pelletCount=1, weaponPrefab_rb="Luger_RB", weaponPrefab_onplayer="Luger_OnPlayer" },
    new WeaponData { name="HK_USP", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=28f, bodyDamage=18f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=350, magSize=12, pelletCount=1, weaponPrefab_rb="HKUSP_RB", weaponPrefab_onplayer="HKUSP_OnPlayer" },
    new WeaponData { name="Sig_Saguer_P226", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=28f, bodyDamage=18f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=350, magSize=12, pelletCount=1, weaponPrefab_rb="SigP226_RB", weaponPrefab_onplayer="SigP226_OnPlayer" },
    new WeaponData { name="Baretta_93_R", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=30f, bodyDamage=20f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=400, magSize=15, pelletCount=1, weaponPrefab_rb="Beretta93_RB", weaponPrefab_onplayer="Beretta93_OnPlayer" },
    new WeaponData { name="Bowie_Knife", weaponType=WeaponType.melee, treeDamage=10f, oreDamage=5f, headDamage=40f, bodyDamage=35f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Bowie_RB", weaponPrefab_onplayer="Bowie_OnPlayer" },
    new WeaponData { name="Kabar_1250", weaponType=WeaponType.melee, treeDamage=12f, oreDamage=5f, headDamage=45f, bodyDamage=35f, legDamage=25f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Kabar_RB", weaponPrefab_onplayer="Kabar_OnPlayer" },
    new WeaponData { name="Telescopic_Baton", weaponType=WeaponType.melee, treeDamage=5f, oreDamage=2f, headDamage=30f, bodyDamage=25f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Baton_RB", weaponPrefab_onplayer="Baton_OnPlayer" },
    new WeaponData { name="Tactic_Shovel", weaponType=WeaponType.melee, treeDamage=8f, oreDamage=5f, headDamage=35f, bodyDamage=30f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Shovel_RB", weaponPrefab_onplayer="Shovel_OnPlayer" },
    new WeaponData { name="Kabar_Tanto", weaponType=WeaponType.melee, treeDamage=10f, oreDamage=5f, headDamage=38f, bodyDamage=33f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="KabarTanto_RB", weaponPrefab_onplayer="KabarTanto_OnPlayer" },
    new WeaponData { name="MK2_Handbomb", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=120f, bodyDamage=100f, legDamage=60f, isThrowable=true, hasAreaEffect=true, fireRate=0, magSize=1, pelletCount=1, weaponPrefab_rb="MK2_RB", weaponPrefab_onplayer="MK2_OnPlayer" },
    new WeaponData { name="M24_Stick_Grenade", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=130f, bodyDamage=110f, legDamage=70f, isThrowable=true, hasAreaEffect=true, fireRate=0, magSize=1, pelletCount=1, weaponPrefab_rb="StickGrenade_RB", weaponPrefab_onplayer="StickGrenade_OnPlayer" },
    new WeaponData { name="RPG7", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=250f, bodyDamage=200f, legDamage=120f, isThrowable=false, hasAreaEffect=true, fireRate=20, magSize=1, pelletCount=1, weaponPrefab_rb="RPG7_RB", weaponPrefab_onplayer="RPG7_OnPlayer" },
    new WeaponData { name="M_72_LAW", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=220f, bodyDamage=180f, legDamage=100f, isThrowable=false, hasAreaEffect=true, fireRate=15, magSize=1, pelletCount=1, weaponPrefab_rb="LAW_RB", weaponPrefab_onplayer="LAW_OnPlayer" },
    new WeaponData { name="MK_153_SMAW", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=280f, bodyDamage=220f, legDamage=130f, isThrowable=false, hasAreaEffect=true, fireRate=12, magSize=1, pelletCount=1, weaponPrefab_rb="SMAW_RB", weaponPrefab_onplayer="SMAW_OnPlayer" },
    new WeaponData { name="MK_153_SMAW_Rocket", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=300f, bodyDamage=240f, legDamage=150f, isThrowable=true, hasAreaEffect=true, fireRate=0, magSize=1, pelletCount=1, weaponPrefab_rb="SMAW_Rocket_RB", weaponPrefab_onplayer="SMAW_Rocket_OnPlayer" },
    new WeaponData { name="PKM", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=42f, bodyDamage=30f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=650, magSize=100, pelletCount=1, weaponPrefab_rb="PKM_RB", weaponPrefab_onplayer="PKM_OnPlayer" },
    new WeaponData { name="MG42", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=40f, bodyDamage=28f, legDamage=16f, isThrowable=false, hasAreaEffect=false, fireRate=1200, magSize=250, pelletCount=1, weaponPrefab_rb="MG42_RB", weaponPrefab_onplayer="MG42_OnPlayer" },
    new WeaponData { name="M249", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=38f, bodyDamage=26f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=750, magSize=200, pelletCount=1, weaponPrefab_rb="M249_RB", weaponPrefab_onplayer="M249_OnPlayer" },
    new WeaponData { name="AW_338", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=120f, bodyDamage=90f, legDamage=60f, isThrowable=false, hasAreaEffect=false, fireRate=50, magSize=5, pelletCount=1, weaponPrefab_rb="AW338_RB", weaponPrefab_onplayer="AW338_OnPlayer" },
    new WeaponData { name="Blaser_R93", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=110f, bodyDamage=85f, legDamage=55f, isThrowable=false, hasAreaEffect=false, fireRate=60, magSize=5, pelletCount=1, weaponPrefab_rb="Blaser_RB", weaponPrefab_onplayer="Blaser_OnPlayer" },
    new WeaponData { name="Desert_Tech_SRS", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=125f, bodyDamage=95f, legDamage=60f, isThrowable=false, hasAreaEffect=false, fireRate=45, magSize=5, pelletCount=1, weaponPrefab_rb="SRS_RB", weaponPrefab_onplayer="SRS_OnPlayer" },
    new WeaponData { name="SVD_Drogunov", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=95f, bodyDamage=70f, legDamage=50f, isThrowable=false, hasAreaEffect=false, fireRate=150, magSize=10, pelletCount=1, weaponPrefab_rb="SVD_RB", weaponPrefab_onplayer="SVD_OnPlayer" },
    new WeaponData { name="JNG_90", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=115f, bodyDamage=88f, legDamage=58f, isThrowable=false, hasAreaEffect=false, fireRate=55, magSize=5, pelletCount=1, weaponPrefab_rb="JNG90_RB", weaponPrefab_onplayer="JNG90_OnPlayer" },
    new WeaponData { name="STEYR_SSG69", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=105f, bodyDamage=80f, legDamage=55f, isThrowable=false, hasAreaEffect=false, fireRate=65, magSize=5, pelletCount=1, weaponPrefab_rb="SSG69_RB", weaponPrefab_onplayer="SSG69_OnPlayer" },
    new WeaponData { name="Barret_M82A1", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=150f, bodyDamage=100f, legDamage=50f, isThrowable=false, hasAreaEffect=false, fireRate=40, magSize=5, pelletCount=1, weaponPrefab_rb="Barret_RB", weaponPrefab_onplayer="Barret_OnPlayer" },
    new WeaponData { name="Mosin_Nagant_Pistol", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=60f, bodyDamage=45f, legDamage=30f, isThrowable=false, hasAreaEffect=false, fireRate=80, magSize=1, pelletCount=1, weaponPrefab_rb="MosinPistol_RB", weaponPrefab_onplayer="MosinPistol_OnPlayer" },
    new WeaponData { name="M1_Grand", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=45f, bodyDamage=32f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=400, magSize=8, pelletCount=1, weaponPrefab_rb="M1Garand_RB", weaponPrefab_onplayer="M1Garand_OnPlayer" },
    new WeaponData { name="M14_Classic", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=48f, bodyDamage=34f, legDamage=22f, isThrowable=false, hasAreaEffect=false, fireRate=450, magSize=20, pelletCount=1, weaponPrefab_rb="M14_RB", weaponPrefab_onplayer="M14_OnPlayer" },
    new WeaponData { name="M14_EBR", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=50f, bodyDamage=36f, legDamage=22f, isThrowable=false, hasAreaEffect=false, fireRate=500, magSize=20, pelletCount=1, weaponPrefab_rb="M14EBR_RB", weaponPrefab_onplayer="M14EBR_OnPlayer" },
    new WeaponData { name="PSS_Assassin_Pistol", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=40f, bodyDamage=30f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=250, magSize=6, pelletCount=1, weaponPrefab_rb="PSS_RB", weaponPrefab_onplayer="PSS_OnPlayer" },
    new WeaponData { name="Weldrod_Mark1", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=35f, bodyDamage=25f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=120, magSize=6, pelletCount=1, weaponPrefab_rb="Weldrod_RB", weaponPrefab_onplayer="Weldrod_OnPlayer" },
    new WeaponData { name="Gerber_LMF", weaponType=WeaponType.melee, treeDamage=12f, oreDamage=5f, headDamage=42f, bodyDamage=35f, legDamage=22f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Gerber_RB", weaponPrefab_onplayer="Gerber_OnPlayer" },
    new WeaponData { name="SOG_HAWK", weaponType=WeaponType.melee, treeDamage=10f, oreDamage=4f, headDamage=38f, bodyDamage=32f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="SOG_RB", weaponPrefab_onplayer="SOG_OnPlayer" },
    new WeaponData { name="Halmak_Komando", weaponType=WeaponType.melee, treeDamage=14f, oreDamage=6f, headDamage=48f, bodyDamage=40f, legDamage=26f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Halmak_RB", weaponPrefab_onplayer="Halmak_OnPlayer" },
    new WeaponData { name="Famas", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=33f, bodyDamage=23f, legDamage=14f, isThrowable=false, hasAreaEffect=false, fireRate=900, magSize=25, pelletCount=1, weaponPrefab_rb="Famas_RB", weaponPrefab_onplayer="Famas_OnPlayer" },
    new WeaponData { name="G36_K", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=34f, bodyDamage=24f, legDamage=15f, isThrowable=false, hasAreaEffect=false, fireRate=750, magSize=30, pelletCount=1, weaponPrefab_rb="G36K_RB", weaponPrefab_onplayer="G36K_OnPlayer" },
    new WeaponData { name="LVOA-C", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=36f, bodyDamage=26f, legDamage=16f, isThrowable=false, hasAreaEffect=false, fireRate=720, magSize=30, pelletCount=1, weaponPrefab_rb="LVOAC_RB", weaponPrefab_onplayer="LVOAC_OnPlayer" },
    new WeaponData { name="M48_Cyclone", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=40f, bodyDamage=28f, legDamage=18f, isThrowable=false, hasAreaEffect=false, fireRate=650, magSize=20, pelletCount=1, weaponPrefab_rb="M48Cyclone_RB", weaponPrefab_onplayer="M48Cyclone_OnPlayer" },
    new WeaponData { name="M48_Falcon", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=42f, bodyDamage=30f, legDamage=20f, isThrowable=false, hasAreaEffect=false, fireRate=600, magSize=20, pelletCount=1, weaponPrefab_rb="M48Falcon_RB", weaponPrefab_onplayer="M48Falcon_OnPlayer" },
    new WeaponData { name="M24_Apply", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=110f, bodyDamage=85f, legDamage=55f, isThrowable=false, hasAreaEffect=false, fireRate=50, magSize=5, pelletCount=1, weaponPrefab_rb="M24_RB", weaponPrefab_onplayer="M24_OnPlayer" },
    new WeaponData { name="HK_MP7", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=26f, bodyDamage=18f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=950, magSize=40, pelletCount=1, weaponPrefab_rb="MP7_RB", weaponPrefab_onplayer="MP7_OnPlayer" },
    new WeaponData { name="Benelli_M2", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=60f, bodyDamage=45f, legDamage=28f, isThrowable=false, hasAreaEffect=true, fireRate=85, magSize=7, pelletCount=8, weaponPrefab_rb="BenelliM2_RB", weaponPrefab_onplayer="BenelliM2_OnPlayer" },
    new WeaponData { name="Maverick_88", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=55f, bodyDamage=40f, legDamage=25f, isThrowable=false, hasAreaEffect=true, fireRate=75, magSize=6, pelletCount=8, weaponPrefab_rb="Maverick88_RB", weaponPrefab_onplayer="Maverick88_OnPlayer" },
    new WeaponData { name="Weatherby_Orion", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=70f, bodyDamage=55f, legDamage=35f, isThrowable=false, hasAreaEffect=true, fireRate=65, magSize=2, pelletCount=10, weaponPrefab_rb="Weatherby_RB", weaponPrefab_onplayer="Weatherby_OnPlayer" },
    new WeaponData { name="Government", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=32f, bodyDamage=22f, legDamage=14f, isThrowable=false, hasAreaEffect=false, fireRate=300, magSize=7, pelletCount=1, weaponPrefab_rb="Government_RB", weaponPrefab_onplayer="Government_OnPlayer" },
    new WeaponData { name="Beretta_M9", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=26f, bodyDamage=18f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=350, magSize=15, pelletCount=1, weaponPrefab_rb="BerettaM9_RB", weaponPrefab_onplayer="BerettaM9_OnPlayer" },
    new WeaponData { name="X13", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=24f, bodyDamage=16f, legDamage=10f, isThrowable=false, hasAreaEffect=false, fireRate=400, magSize=13, pelletCount=1, weaponPrefab_rb="X13_RB", weaponPrefab_onplayer="X13_OnPlayer" },
    new WeaponData { name="Canit_TP9_Elite_Combat", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=28f, bodyDamage=20f, legDamage=12f, isThrowable=false, hasAreaEffect=false, fireRate=380, magSize=18, pelletCount=1, weaponPrefab_rb="CanikTP9_RB", weaponPrefab_onplayer="CanikTP9_OnPlayer" },
    new WeaponData { name="Mauser_1918T_Gewehr", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=140f, bodyDamage=110f, legDamage=80f, isThrowable=false, hasAreaEffect=false, fireRate=20, magSize=1, pelletCount=1, weaponPrefab_rb="Mauser1918_RB", weaponPrefab_onplayer="Mauser1918_OnPlayer" },
    new WeaponData { name="Nagant_M1895", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=30f, bodyDamage=22f, legDamage=14f, isThrowable=false, hasAreaEffect=false, fireRate=250, magSize=7, pelletCount=1, weaponPrefab_rb="Nagant1895_RB", weaponPrefab_onplayer="Nagant1895_OnPlayer" },
    new WeaponData { name="2MM_Kalibri", weaponType=WeaponType.sidearm, treeDamage=0f, oreDamage=0f, headDamage=8f, bodyDamage=5f, legDamage=3f, isThrowable=false, hasAreaEffect=false, fireRate=150, magSize=1, pelletCount=1, weaponPrefab_rb="2mm_RB", weaponPrefab_onplayer="2mm_OnPlayer" },
    new WeaponData { name="Mikor_MGL", weaponType=WeaponType.rifle, treeDamage=0f, oreDamage=0f, headDamage=140f, bodyDamage=120f, legDamage=80f, isThrowable=false, hasAreaEffect=true, fireRate=60, magSize=6, pelletCount=1, weaponPrefab_rb="MGL_RB", weaponPrefab_onplayer="MGL_OnPlayer" },
    new WeaponData { name="Tarran_Tactical", weaponType=WeaponType.melee, treeDamage=15f, oreDamage=6f, headDamage=50f, bodyDamage=42f, legDamage=28f, isThrowable=false, hasAreaEffect=false, fireRate=1, magSize=0, pelletCount=1, weaponPrefab_rb="Tarran_RB", weaponPrefab_onplayer="Tarran_OnPlayer" },
}; // Your WeaponData list

    [MenuItem("Tools/Generate Weapon ScriptableObjects")]
    static void ShowWindow()
    {
        GetWindow<WeaponsSOAssigner>("Weapon SO Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Weapon ScriptableObject Generator", EditorStyles.boldLabel);

        weaponsRootPath = EditorGUILayout.TextField("Weapons Root Path", weaponsRootPath);

        if (GUILayout.Button("Generate ScriptableObjects and Assign to Prefabs"))
        {
            GenerateAndAssign();
        }
    }

    void GenerateAndAssign()
    {
        int count = 0;
        foreach (var weapon in weapons)
        {
            count++;
            string weaponFolder = Path.Combine(weaponsRootPath, weapon.name);

            if (!Directory.Exists(weaponFolder))
            {
                Debug.LogWarning($"Folder not found for {weapon.name}: {weaponFolder}");
                continue;
            }

            // Create ScriptableObject in this folder
            string soPath = Path.Combine(weaponFolder, weapon.name + ".asset");
            RangedWeapon weaponSO;

            weaponSO = ScriptableObject.CreateInstance<RangedWeapon>();


            // Populate fields from WeaponData
            weaponSO.ItemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(weaponFolder, weapon.name + "_Icon.png"));
            weaponSO.ItemName = weapon.name;
            weaponSO.Type = ItemType.weapon;
            weaponSO.ItemDescription = "Fires " + weapon.magSize + " rounds at " + weapon.fireRate + " rounds per minute. ";
            weaponSO.WeaponType = weapon.weaponType;
            weaponSO.treeDamage = weapon.treeDamage;
            weaponSO.oreDamage = weapon.oreDamage;
            weaponSO.headDamage = weapon.headDamage;
            weaponSO.bodyDamage = weapon.bodyDamage;
            weaponSO.legDamage = weapon.legDamage;
            weaponSO.hasAreaEffect = weapon.hasAreaEffect;
            weaponSO.isAutomatic = weapon.isAutomatic;
            weaponSO.position = weapon.position;
            weaponSO.rotation = weapon.rotation;
            weaponSO.scale = weapon.scale;
            weaponSO.storedPosition = weapon.storedPosition;
            weaponSO.storedRotation = weapon.storedRotation;
            weaponSO.storedScale = weapon.storedScale;

            // Save the ScriptableObject
            AssetDatabase.CreateAsset(weaponSO, soPath);

            // --- Assign ScriptableObject to prefabs ---
            string onPlayerPrefabPath = Path.Combine(weaponFolder, "onplayer_" + weapon.name + ".prefab");
            string rbPrefabPath = Path.Combine(weaponFolder, "rb_" + weapon.name + ".prefab");

            GameObject onPlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(onPlayerPrefabPath);
            GameObject rbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(rbPrefabPath);
            GameObject ikTarget = onPlayerPrefab.transform.GetChild(0).gameObject;

            if (onPlayerPrefab != null)
            {
                RangedWeaponBehaviour behaviour = onPlayerPrefab.GetComponent<RangedWeaponBehaviour>();
                if (behaviour != null)
                {
                    behaviour.baseitem = weaponSO;
                    EditorUtility.SetDirty(onPlayerPrefab);
                    weaponSO.weaponPrefab_onplayer = onPlayerPrefab;
                    behaviour.ik_target = ikTarget.transform;
                }
            }

            if (rbPrefab != null)
            {
                RangedWeaponBehaviour behaviour = rbPrefab.GetComponent<RangedWeaponBehaviour>();
                if (behaviour != null)
                {
                    behaviour.baseitem = weaponSO;
                    EditorUtility.SetDirty(rbPrefab);
                    weaponSO.weaponPrefab_rb = rbPrefab;
                }
            }

            Debug.Log($"Created ScriptableObject and assigned to prefabs for {weapon.name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All weapons processed successfully!");
    }
}

