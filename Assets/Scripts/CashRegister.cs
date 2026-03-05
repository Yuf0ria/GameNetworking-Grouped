using Unity.VisualScripting;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Cash"))
        {
            Destroy(other.gameObject);
            
            //then something to add server cash (cash is serverwide and is shared by everyone in server)
        }
    }
}
