using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public enum CharacterType
{
    Female,
    Male
}
public class UI_Lobby : MonoBehaviour
{
    public static CharacterType SelectedCharacterType = CharacterType.Female;

    public GameObject FemaleCharacter;
    public GameObject MaleCharacter;

    public InputField NicknameInputFieldUI;
    public InputField RoomIDInputFieldUI;
    private void Start()
    {
        FemaleCharacter.SetActive(SelectedCharacterType == CharacterType.Female);
        MaleCharacter.SetActive(SelectedCharacterType == CharacterType.Male);
    }
    // 방만들기 버튼을 눌렀을때 호출되는 함수
    public void OnClickMakeRoomButton()
    {
        string nickname = NicknameInputFieldUI.text;
        string roomID = RoomIDInputFieldUI.text;

        if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(roomID))
        {
            Debug.Log("입력하세요.");
            return;
        }

        PhotonNetwork.NickName = nickname;
        // [룸 옵션 설정]
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;   // 입장 가능한 최대 플레이어 수
        roomOptions.IsVisible = true; // 로비에서 방 목록에 노출할 것인가?
        roomOptions.IsOpen = true; // 방에 다른 플레이어가 들어올 수 있는가?
        roomOptions.EmptyRoomTtl = 1000 * 20;
        roomOptions.CustomRoomProperties = new Hashtable() // 룸 커스텀 프로퍼티 (플레이어 커스텀 프로피터)
        {
            {"MasterNickname", nickname}
        };
        // 로비에서 공개적으로 표시될 룸 커스텀 프로퍼티의 키를 정의
        // -> 방을 검색하거나 선택할 때 사용자에게 유용한 정보를 제공하기 위해 사용
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "MasterNickname" };
        // roomOptions.PlayerTtl = 100 * 30; 잘 안씀
        PhotonNetwork.JoinOrCreateRoom(roomID, roomOptions, TypedLobby.Default); // 방이 있다면 입장하고 없다면 만드는 것
    }

    // 캐릭터 타입 버튼 눌렀을때 호출되는 함수

    public void OnClickMaleButton() { ONClickCharacterTypeButton(CharacterType.Male); }
    public void OnClickFemaleButton() => ONClickCharacterTypeButton(CharacterType.Female);

    public void ONClickCharacterTypeButton(CharacterType characterType)
    {
        SelectedCharacterType = characterType;
        FemaleCharacter.SetActive(SelectedCharacterType == CharacterType.Female);
        MaleCharacter.SetActive(SelectedCharacterType == CharacterType.Male);
    }

    public void OnNicknameValueChanged(string newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            return;
        }
        PhotonNetwork.NickName = newValue;
    }
}