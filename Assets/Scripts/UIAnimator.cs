
using UnityEngine;

public class UIAnimator : MonoBehaviour
{
    public GameObject gridContainer;
    public GameObject uiTop;
    public GameObject winScreen;
    public float moveDistance = 700f;
    public float duration = 0.5f; // Adjust animation duration

    public GameObject blueParticle;
    public GameObject redParticle;
    public GameObject greenParticle;
    public GameObject yellowParticle;
    public GameObject vaseParticle1;
    public GameObject vaseParticle2;
    public GameObject vaseParticle3;
    public GameObject stoneParticle1;
    public GameObject stoneParticle2;
    public GameObject stoneParticle3;
    public GameObject boxParticle1;
    public GameObject boxParticle2;
    public GameObject boxParticle3;

    public GameObject rocketSmokeTrail;
    public GameObject rocketStarTrail;



    private Vector3 startPos1;
    private Vector3 startPos2;

    //private Animation starAnimation;

    

    public void AnimateUI(){   
        startPos1 = gridContainer.transform.position;
        startPos2 = uiTop.transform.position;

        gridContainer.transform.position += new Vector3(10, 0, 0);
        uiTop.transform.position += new Vector3(0, 10, 0);


        gridContainer.SetActive(true);
        uiTop.SetActive(true);

        LeanTween.move(gridContainer, startPos1, duration).setEase(LeanTweenType.easeOutQuad);
        LeanTween.move(uiTop, startPos2, duration).setEase(LeanTweenType.easeOutQuad);


    }

    public void AnimateDestructionOnTile(Tile tile){

        GameObject[] particles = getTilesPrefab(tile);
    
        if (particles == null || particles.Length == 0)
            return;

        foreach (GameObject particlePrefab in particles)
        {
            GameObject particleInstance = Instantiate(particlePrefab, tile.transform.position, Quaternion.identity);
            ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Play();
                Destroy(particleInstance, 0.5f);
            }
        }
    }

    private GameObject[] getTilesPrefab(Tile tile){
        string type = tile.tileType;
        if (type == "b") return new GameObject[] { blueParticle };
        if (type == "g") return new GameObject[] { greenParticle };
        if (type == "y") return new GameObject[] { yellowParticle };
        if (type == "r") return new GameObject[] { redParticle };
        if (type == "bo") return new GameObject[] { boxParticle1, boxParticle2, boxParticle3};
        if (type == "s") return new GameObject[] { stoneParticle1, stoneParticle2, stoneParticle3 };
        if (type == "v" || type == "cv") return new GameObject[] { vaseParticle1, vaseParticle2, vaseParticle3 };
        return null;
    }


    public void AnimateRocketMove(Tile tile, int directionX, int directionY) {
        // Instantiate and attach Rocket Smoke Trail
        GameObject smokeInstance = Instantiate(rocketSmokeTrail, tile.transform.position, Quaternion.identity);
        smokeInstance.transform.SetParent(tile.transform);
        smokeInstance.transform.localPosition = Vector3.zero;
        
        ParticleSystem smokePS = smokeInstance.GetComponent<ParticleSystem>();
        SetRocketTrailVelocity(smokePS, directionX, directionY, 3f); // Modify velocity

        if (smokePS != null) {
            smokePS.Play();
        }

        // Instantiate and attach Rocket Star Trail
        GameObject starInstance = Instantiate(rocketStarTrail, tile.transform.position, Quaternion.identity);
        starInstance.transform.SetParent(tile.transform);
        starInstance.transform.localPosition = Vector3.zero;
        
        ParticleSystem starPS = starInstance.GetComponent<ParticleSystem>();
        SetRocketTrailVelocity(starPS, directionX, directionY, 0.5f); // Different speed if needed

        if (starPS != null) {
            starPS.Play();
        }
    }


    public void SetRocketTrailVelocity(ParticleSystem ps, int directionX, int directionY, float velocity){
        var velocityModule = ps.velocityOverLifetime;
        if(directionX == 1){
            velocityModule.x = new ParticleSystem.MinMaxCurve(-1*velocity);
            velocityModule.y = new ParticleSystem.MinMaxCurve(0f);
        }else if(directionX == -1){
            velocityModule.x = new ParticleSystem.MinMaxCurve(velocity);
            velocityModule.y = new ParticleSystem.MinMaxCurve(0f);
        }else if(directionY == 1){
            velocityModule.x = new ParticleSystem.MinMaxCurve(0f);
            velocityModule.y = new ParticleSystem.MinMaxCurve(velocity);
        }else if(directionY == -1){
            velocityModule.x = new ParticleSystem.MinMaxCurve(0f);
            velocityModule.y = new ParticleSystem.MinMaxCurve(-1*velocity);
        }else{
            return;
        }
    }


    public void WinAnimation(){
        GameObject star = winScreen.transform.Find("Star")?.gameObject;
        GameObject ribbon = winScreen.transform.Find("Ribbon")?.gameObject;


        Vector3 ribbonstartpose = ribbon.transform.position;
        ribbon.transform.position +=new Vector3(0, 10, 0);

        winScreen.SetActive(true);

        LeanTween.move(ribbon, ribbonstartpose, 0.5f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.scale(star, new Vector3(1.5f,1.5f,1.5f),2f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.scale(star, new Vector3(1.5f,1.5f,1.5f), 2f)
        .setEase(LeanTweenType.easeOutElastic)
        .setOnComplete(() => {
            star.transform.localScale = Vector3.zero;  // Reset to initial scale
        });
    }


}




