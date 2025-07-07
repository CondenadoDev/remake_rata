using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    public GameObject projectilePrefab;  // Asigna el prefab del proyectil en el Inspector
    public int poolSize = 20;            // Número inicial de proyectiles en el pool

    private List<GameObject> pool;

    void Awake()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject proj = Instantiate(projectilePrefab);
            proj.SetActive(false);
            pool.Add(proj);
        }
    }

    // Devuelve un proyectil inactivo del pool. Si no hay ninguno, opcionalmente lo expande.
    public GameObject GetPooledProjectile()
    {
        foreach (GameObject proj in pool)
        {
            if (!proj.activeInHierarchy)
                return proj;
        }
        // Si no hay proyectiles disponibles, se crea uno nuevo (puedes ajustar este comportamiento)
        GameObject newProj = Instantiate(projectilePrefab);
        newProj.SetActive(false);
        pool.Add(newProj);
        return newProj;
    }
}
