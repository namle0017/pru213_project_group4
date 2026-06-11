using System;
using System.Reflection;
using UnityEngine;

public class SelectedVehicleLoader : MonoBehaviour
{
    [Serializable]
    private class VehiclePrefabEntry
    {
        public string vehicleId;
        public GameObject vehiclePrefab;
    }

    [Header("Scene References")]
    [SerializeField] private Transform placeholderPlayer;
    [SerializeField] private GameSession gameSession;
    [SerializeField] private InfiniteTerrain infiniteTerrain;
    [SerializeField] private PickupSpawnerRuntime pickupSpawnerRuntime;
    [SerializeField] private Component playerCamera;

    [Header("Vehicle Prefabs")]
    [SerializeField] private VehiclePrefabEntry[] vehiclePrefabs;

    [Header("Options")]
    [SerializeField] private bool destroyPlaceholderPlayer = true;

    private void Awake()
    {
        EnsureReferences();
        SpawnSelectedVehicle();
    }

    private void EnsureReferences()
    {
        if (placeholderPlayer == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                placeholderPlayer = taggedPlayer.transform;
            }
        }

        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }

        if (infiniteTerrain == null)
        {
            infiniteTerrain = FindFirstObjectByType<InfiniteTerrain>();
        }

        if (pickupSpawnerRuntime == null)
        {
            pickupSpawnerRuntime = FindFirstObjectByType<PickupSpawnerRuntime>();
        }

        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        }
    }

    private void SpawnSelectedVehicle()
    {
        if (placeholderPlayer == null)
        {
            Debug.LogWarning("SelectedVehicleLoader: Chua gan placeholderPlayer.");
            return;
        }

        string selectedVehicleId = SaveSystem.LoadSelectedVehicle();
        Debug.Log("SelectedVehicleLoader: Requested vehicle = " + selectedVehicleId);
        GameObject selectedPrefab = GetVehiclePrefab(selectedVehicleId);

        if (selectedPrefab == null)
        {
            Debug.LogWarning("SelectedVehicleLoader: Khong tim thay prefab cho xe da chon, fallback ve Basic Car.");
            selectedVehicleId = VehicleIds.BasicCar;
            selectedPrefab = GetVehiclePrefab(selectedVehicleId);
        }

        if (selectedPrefab == null)
        {
            Debug.LogError("SelectedVehicleLoader: Khong co prefab xe nao hop le de spawn.");
            return;
        }

        Transform parent = placeholderPlayer.parent;
        Debug.Log("SelectedVehicleLoader: Prefab asset = " + selectedPrefab.name + " | type = " + selectedPrefab.GetType().FullName);

        UnityEngine.Object spawnedObject = UnityEngine.Object.Instantiate((UnityEngine.Object)selectedPrefab);
        GameObject spawnedVehicle = ExtractGameObject(spawnedObject);

        if (spawnedVehicle == null)
        {
            Debug.LogError(
                "SelectedVehicleLoader: Instantiate xong nhung object khong phai GameObject cho vehicleId = "
                + selectedVehicleId
                + " | spawnedType = "
                + (spawnedObject == null ? "null" : spawnedObject.GetType().FullName));
            return;
        }

        if (parent != null)
        {
            spawnedVehicle.transform.SetParent(parent, true);
        }

        spawnedVehicle.transform.SetPositionAndRotation(placeholderPlayer.position, placeholderPlayer.rotation);

        spawnedVehicle.name = placeholderPlayer.name;
        ApplyPlayerTagRecursive(spawnedVehicle.transform);

        if (gameSession != null)
        {
            gameSession.SetPlayer(spawnedVehicle.transform);
        }

        if (infiniteTerrain != null)
        {
            infiniteTerrain.SetPlayer(spawnedVehicle.transform);
        }

        if (pickupSpawnerRuntime != null)
        {
            pickupSpawnerRuntime.SetPlayer(spawnedVehicle.transform);
        }

        RebindCamera(spawnedVehicle.transform);

        if (destroyPlaceholderPlayer)
        {
            Destroy(placeholderPlayer.gameObject);
        }
        else
        {
            placeholderPlayer.gameObject.SetActive(false);
        }

        Debug.Log("SelectedVehicleLoader: Spawned vehicle " + selectedVehicleId + ".");
    }

    private GameObject GetVehiclePrefab(string vehicleId)
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            return null;
        }

        foreach (VehiclePrefabEntry entry in vehiclePrefabs)
        {
            if (entry != null && entry.vehicleId == vehicleId && entry.vehiclePrefab != null)
            {
                return entry.vehiclePrefab;
            }
        }

        foreach (VehiclePrefabEntry entry in vehiclePrefabs)
        {
            if (entry != null && entry.vehicleId == VehicleIds.BasicCar && entry.vehiclePrefab != null)
            {
                return entry.vehiclePrefab;
            }
        }

        return vehiclePrefabs[0] != null ? vehiclePrefabs[0].vehiclePrefab : null;
    }

    private static GameObject ExtractGameObject(UnityEngine.Object source)
    {
        if (source is GameObject gameObject)
        {
            return gameObject;
        }

        if (source is Component component)
        {
            return component.gameObject;
        }

        return null;
    }

    private void RebindCamera(Transform newPlayer)
    {
        if (playerCamera == null || newPlayer == null)
        {
            return;
        }

        Type cameraType = playerCamera.GetType();
        PropertyInfo followProperty = cameraType.GetProperty("Follow", BindingFlags.Public | BindingFlags.Instance);
        if (followProperty != null && followProperty.CanWrite)
        {
            followProperty.SetValue(playerCamera, newPlayer);
        }

        PropertyInfo targetProperty = cameraType.GetProperty("Target", BindingFlags.Public | BindingFlags.Instance);
        if (targetProperty == null || !targetProperty.CanRead || !targetProperty.CanWrite)
        {
            return;
        }

        object targetValue = targetProperty.GetValue(playerCamera);
        if (targetValue == null)
        {
            return;
        }

        PropertyInfo trackingTargetProperty = targetValue.GetType().GetProperty("TrackingTarget", BindingFlags.Public | BindingFlags.Instance);
        if (trackingTargetProperty == null || !trackingTargetProperty.CanWrite)
        {
            return;
        }

        trackingTargetProperty.SetValue(targetValue, newPlayer);
        targetProperty.SetValue(playerCamera, targetValue);
    }

    private static void ApplyPlayerTagRecursive(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            child.tag = "Player";
        }
    }
}
