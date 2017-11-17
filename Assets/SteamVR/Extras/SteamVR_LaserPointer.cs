﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public struct PointerEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);


public class SteamVR_LaserPointer : MonoBehaviour
{
    public bool active = true;
    public Color color;
    public float thickness = 0.002f;
    public GameObject holder;
    public GameObject pointer;
    bool isActive = false;
    public bool addRigidBody = false;
    public Transform reference;
    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;

    private bool isColliding = false;
    private bool isPressed = false;

    private GameObject previousContact;

    public SiteUI siteUI;

	// Use this for initialization
	void Start ()
    {
        if (holder == null)
        {
            holder = new GameObject();
        }
        
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
		holder.transform.localRotation = Quaternion.identity;

        if (pointer == null)
        {
            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
		
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
		pointer.transform.localRotation = Quaternion.identity;
		BoxCollider collider = pointer.GetComponent<BoxCollider>();
        if (collider)
        {
            Object.Destroy(collider);
        }
        /*
        if (addRigidBody)
        {
            if (collider)
            {
                collider.isTrigger = true;
            }
            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if(collider)
            {
                Object.Destroy(collider);
            }
        }
        */
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
	}

    public virtual void OnPointerIn(PointerEventArgs e)
    {
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerOut(PointerEventArgs e)
    {
        if (PointerOut != null)
            PointerOut(this, e);
    }


    // Update is called once per frame
	void Update ()
    {
        if (!isActive)
        {
            isActive = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }

        float dist = 100f;

        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();

        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("UI");
        bool bHit = Physics.Raycast(raycast, out hit, Mathf.Infinity,layerMask);

        if (bHit)
        {
            GameObject collidingObject = hit.collider.gameObject;
            if (previousContact != null)
            {
                previousContact.GetComponent<Image>().color = Color.white;
                collidingObject.GetComponent<Image>().color = Color.black;
            }

            previousContact = collidingObject;
        }




            /*


            if (!isColliding)
            {
                isColliding = true;
                previousContact = hit.collider.gameObject;
                hit.collider.gameObject.GetComponent<Image>().color = Color.black;
                if (controller.triggerPressed)
                {
                    if (!isPressed)
                    {
                        isPressed = true;
                        Debug.Log("selected: " + hit.collider.gameObject.name);
                        SiteUI manager = hit.collider.gameObject.transform.parent.GetComponent<SiteUI>();
                        int index = hit.collider.gameObject.GetComponent<SiteButton>().siteIndex;
                        manager.MoveButtons(index);
                        Debug.Log("manager: " + manager.gameObject.name);
                        Debug.Log("index: " + index);
                    } 
                } else
                {
                    isPressed = false;
                }
                
            }
        } else
        {
            isColliding = false;
            previousContact.GetComponent<Image>().color = Color.white;
        }*/

        /*
        if(previousContact && previousContact != hit.transform)
        {
            PointerEventArgs args = new PointerEventArgs();
            if (controller != null)
            {
                args.controllerIndex = controller.controllerIndex;
            }
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
            previousContact = null;
        }
        if(bHit && previousContact != hit.transform)
        {
            PointerEventArgs argsIn = new PointerEventArgs();
            if (controller != null)
            {
                argsIn.controllerIndex = controller.controllerIndex;
            }
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerIn(argsIn);
            previousContact = hit.transform;
        }
        if(!bHit)
        {
            previousContact = null;
        }

        if (bHit && controller != null && controller.triggerPressed)
        {
            pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
        }
        else
        {
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
        }

        pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        */
    }
}