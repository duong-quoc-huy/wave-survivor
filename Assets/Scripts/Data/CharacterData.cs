using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterId; 
    public string characterName;

    [Header("Base Stats")]
    public float baseMaxHP;
    public float baseAttack;
    public float baseMoveSpeed;

    [Header("Skill 1 Settings")]
    public string skill1Name;
    public Sprite skill1Icon;
    public float skill1Cooldown = 5f;
    public float skill1Duration = 5f;

    [Header("Skill 2 Settings")]
    public string skill2Name;
    public Sprite skill2Icon;
    public float skill2Cooldown = 12f;
    public VideoClip skill2CutsceneVideo;
}