using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum ParticleType{
    BURN = 0,
    BOOM,
    WATER,
    ELECTRIC
}


public class BlockParticlePool : MonoBehaviour {
    [SerializeField] private Particle[] ParticlePrefab = new Particle[4];
    private IObjectPool<Particle>[] ParticlePool = new IObjectPool<Particle>[4];
    private ParticleType requestType;

    private void Awake() {
        InitParticlePool();
    }

    public Particle GetParticle(ParticleType type) {
        requestType = type;
        return ParticlePool[(int)type].Get();
    }

    private void InitParticlePool() {
        for(int i = 0; i < ParticlePool.Length; i++)
        ParticlePool[i] = new ObjectPool<Particle>(
            createFunc: () => CreateParticle(),
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: DestroyParticle
            );
    }

    private Particle CreateParticle(){
        Particle newParticle = Instantiate(ParticlePrefab[(int)requestType], this.transform);
        newParticle.InitPool(ParticlePool[(int)requestType]);
        return newParticle;
    }

    private void OnGet(Particle particle) {
        particle.gameObject.SetActive(true);
    }

    private void OnRelease(Particle particle) {
        particle.gameObject.SetActive(false);
    }
    private void DestroyParticle(Particle particle) {
        Destroy(particle.gameObject);
    }
}
