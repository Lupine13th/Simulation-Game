using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    private float m_PopOutDeleteCount = 0.0f;

    private void Update()
    {
        m_PopOutDeleteCount += Time.deltaTime;  //ŠÔ‚ğƒJƒEƒ“ƒg

        if (m_PopOutDeleteCount > 10.0f)        //10•b‚½‚Á‚½‚ç
        {
            Destroy(gameObject);                //íœ
        }
    }

    public void DeleteThisPopup()
    {
        Destroy(gameObject);    //íœ
    }
}
