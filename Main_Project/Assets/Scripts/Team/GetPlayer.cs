using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Team.Fightermanage;
using Battle.Scripts.Value.Data;
using Battle.Scripts.Ai.CharacterCreator;
using Battle.Scripts.ImageManager;
using Battle.Scripts.Value.Data;
using Battle.Scripts.Ai;
using UnityEngine.UI;
//팀 관리 -> 선수영입

namespace Scripts.Team.FighterRandomBuy
{
    public class GetPlayer : MonoBehaviour
    {   
        public State state;

        public RandomCharacter[] randomcharacter;

        public CharacterID[] characterid;

        public GameObject[] fighterimage;

        public TransparentScreenshot buyfighterimage;

        public SaveManager savemanage; 

        public GameObject canvas;
        public GameObject drawfighter;

        public Animator[] anim;
        public AnimationClip clip;

        public List<int> cardopen = new List<int>{0,0,0,0,0,0,0,0,0,0};

        public int cardnumber;
        
        public void Start()
        {
            buyfighterimage.LoadAllSprites();
        }
    
        public void FighterBuy()
        {
            /*for (int i = 0; i < characterid.Length; i++)
            {
                int id = int.Parse(characterid[i].characterKey); 
                id += 10;                          
                characterid[i].characterKey = id.ToString();
            }*/   
            foreach (var fighter in randomcharacter)
            {
                fighter.Randomize();
            }
            
            savemanage.SaveFromButton();
            
            buyfighterimage.CaptureAndSaveAll();
            buyfighterimage.LoadAllSprites();
        }

        public void FighterConfrim(int fighterselect)
        {   
            if (state.deleteList.Count >= 1)
            {
                /*for (int k = 1; k <= state.deleteCount; k++)
                {
                    fighterButtons[state.playerIndexList.Count-k].gameObject.SetActive(true);
                    playerimage[state.playerIndexList.Count-k].GetComponentInChildren<CharacterID>().characterKey = (state.deleteList[0]).ToString();
                    deleteList.RemoveAt(0);
                }*/
                
                state.fighterButtons[state.playerIndexList.Count-state.deleteList.Count].gameObject.SetActive(true);
                //state.playerimage[state.playerIndexList.Count-state.deleteList.Count].GetComponentInChildren<CharacterID>().characterKey = characterid[fighterselect].characterKey;
                characterid[fighterselect].characterKey = (state.deleteList[0]).ToString();

                savemanage.SaveFromButton();
                buyfighterimage.CaptureAndSaveAll();

                state.deleteCount -= 1;
                state.deleteList.RemoveAt(0);

                /*state.playerimage[state.playerIndexList.Count-state.deleteCount].GetComponentInChildren<CharacterID>().characterKey = (state.deleteList[0]).ToString();
                state.playerIndexList[state.playerIndexList.Count-state.deleteCount] = state.deleteList[0];

                state.deleteCount -= 1;
                state.deleteList.RemoveAt(0);*/
                buyfighterimage.LoadAllSprites();
                Debug.Log("검투사 구매");
            }
            else
            {
                Debug.Log("검투사 보유 공간 부족");
            }
        }

        public void CardSelect(int cardnum)
        {   
            cardnumber = cardnum;
            anim[cardnum].SetTrigger("Iscardclick");
            //StartCoroutine(WaitforAnim(cardnum));
            cardopen[cardnum] = 1;
        }

        public void FighterCanvas()
        {   
            if (state.deleteList.Count >= 1)
            { 
                buyfighterimage.LoadAllSprites();
                StartCoroutine(WaitforLoad());
                canvas.SetActive(false);
                drawfighter.SetActive(true);
            }
            else
            {
                Debug.Log("검투사 보유 공간 부족");
            }
        }

        public void ReturnCanvas()
        {
            canvas.SetActive(true);
            drawfighter.SetActive(false);
            buyfighterimage.LoadAllSprites();
            state.changeimage.LoadAllSprites();  
            state.trainimage.LoadAllSprites(); 
            cardopen = cardopen.ConvertAll(x => 0);

            int idvalue = 20;
            foreach(var id in characterid)
            {
                id.characterKey = idvalue.ToString();
                idvalue += 1;
            }
        }

        IEnumerator WaitforLoad()
        {
            yield return new WaitForSeconds(3f);  // 3초 대기
        }
        IEnumerator WaitforAnim(int cardnum)
        {
            yield return new WaitForSeconds(0.9f);
            fighterimage[cardnum].SetActive(true);
        }
    }   
}
