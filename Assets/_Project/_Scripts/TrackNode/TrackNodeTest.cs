using UnityEngine;

public class TrackNodeTest : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int nodeIndex = 0; nodeIndex < transform.childCount; nodeIndex++)
        {
            int nextIndex = nodeIndex + 1;

            if (nextIndex == transform.childCount)
            {
                nextIndex = 0;
            }

            TrackNode currentNode = transform.GetChild(nodeIndex).GetComponent<TrackNode>();
            TrackNode nextNode = transform.GetChild(nextIndex).GetComponent<TrackNode>();

            currentNode.nextNode = nextNode;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
