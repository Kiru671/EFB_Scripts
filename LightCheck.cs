using CMF;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class LightCheck : MonoBehaviour
{
    public GameObject directionalLight;
    public LayerMask shadowLayer;
    private AdvancedWalkerController charController;
    public static bool isRadiant = false;

    // Start is called before the first frame update
    void Awake()
    {
        charController = gameObject.GetComponent<AdvancedWalkerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, directionalLight.transform.position -directionalLight.transform.forward);
        RaycastHit hit;
        

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, shadowLayer)) 
        {

            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

            float angle = Vector3.Angle(hit.normal, -directionalLight.transform.forward);
            if(angle >= 90f) 
            {
                isRadiant = false;
                Debug.Log("Shaded");
                StopCoroutine("EnergyRegen");
            }

        }
        else
        {
            if (!isRadiant)
                StartCoroutine("EnergyRegen", charController.regenTime);
            isRadiant= true;
        }
    }

    public IEnumerator EnergyRegen(float f)
    {
        while (true)
        {
            if (charController.currentJump <= 0)
                charController.currentJump = 0;
            if (charController.IsGrounded() && isRadiant || charController.currentControllerState == AdvancedWalkerController.ControllerState.Grabbing && isRadiant)
                charController.currentJump--;
            Debug.Log("Regenerated");
            yield return new WaitForSeconds(f);
        }
    }

}
