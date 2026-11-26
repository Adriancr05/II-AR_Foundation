# Práctica AR Foundation: juego del coche en Realidad Aumentada

Asignatura: Interfaces Inteligentes  
Universidad: Universidad de La Laguna  
Titulación: Grado en Ingeniería Informática  
Curso: 2026/2027  
Fecha: 13 de noviembre de 2026

Aplicación de Realidad Aumentada para Android desarrollada con **Unity**, **AR Foundation** y **ARCore**, a partir del [Codelab de Google](https://codelabs.developers.google.com/arcore-unity-ar-foundation#0). Un coche virtual se coloca sobre una superficie real y persigue una retícula situada en el centro de la pantalla. El objetivo es recoger **paquetes** (+1 punto) y esquivar **arañas** (−1 punto).

## Índice

1. [Descripción](#descripción)
2. [Mecánica del juego](#mecánica-del-juego)
3. [Requisitos](#requisitos)
4. [Compilación y ejecución](#compilación-y-ejecución)
5. [Estructura del proyecto](#estructura-del-proyecto)
6. [Scripts](#scripts)
7. [Configuración de la escena y los prefabs](#configuración-de-la-escena-y-los-prefabs)
8. [Cómo jugar](#cómo-jugar)
9. [Capturas y vídeo](#capturas-y-vídeo)
10. [Créditos y referencias](#créditos-y-referencias)

## Descripción

La práctica tiene dos partes:

1. **Aplicación base de RA** con Unity y AR Foundation + ARCore, siguiendo el Codelab: detección de planos, retícula, coche, paquetes y estimación de luz.
2. **Adaptación del juego**:
   - Cuando el coche toca un **paquete**, suma **1 punto** y aparece otro paquete.
   - Cuando toca una **araña**, resta **1 punto** y aparece otra araña.
   - La **puntuación se muestra en pantalla**.

La araña se configura igual que el paquete: un *prefab* con *collider* en modo *trigger* y un *spawner* que la coloca al azar sobre el plano bloqueado. Lo nuevo es el *tag* y la mecánica de puntos.

## Mecánica del juego

| Elemento | Tag | Al ser tocado por el coche |
|----------|-----|----------------------------|
| Paquete | `Package` | +1 punto, se destruye y el spawner genera otro |
| Araña | `Spider` | −1 punto, se destruye y el spawner genera otra |
| Texto de puntuación | `ScoreText` | Muestra `Score: N` en pantalla |

## Requisitos

- **Unity 6** con el módulo *Android Build Support* (versión exacta en `ProjectSettings/ProjectVersion.txt`).
- Paquetes: AR Foundation, ARCore XR Plugin, XR Simulation (opcional, para probar en el editor), Universal Render Pipeline y TextMesh Pro.
- Dispositivo Android compatible con [ARCore](https://developers.google.com/ar/devices) con depuración USB activada y *Servicios de Google Play para RA* instalados.
- Assets de terceros ya incluidos en el proyecto:
  - *Starter Package* del Codelab (`codelab-starter-package.unitypackage`).
  - [Fuga Spiders](https://assetstore.unity.com/packages/3d/characters/creatures/fuga-spiders-with-destructible-eggs-and-mummy-151921).

## Compilación y ejecución

1. Clona el repositorio y ábrelo con Unity Hub (*Add → Add project from disk*).
2. Abre la escena `Assets/Scenes/SampleScene.unity`.
3. En **File → Build Settings** selecciona **Android** y pulsa *Switch Platform*.
4. En **Project Settings → XR Plug-in Management → Android** activa **ARCore** (el proyecto ya incluye `ARCoreLoader`).
5. Comprueba en **Player Settings**: *Scripting Backend* IL2CPP, arquitectura ARM64 y sin Vulkan en *Graphics APIs*.
6. Conecta el móvil por USB y pulsa **Build And Run**. Los binarios generados se guardan en la carpeta `build/`.

> Para probar sin móvil se puede usar el **XR Simulation Loader** (`Assets/XR/Loaders/SimulationLoader.asset`) en el editor.

## Estructura del proyecto

```
.
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity              # Escena principal
│   ├── Starter Package/                   # Assets y scripts del Codelab + adaptaciones
│   │   ├── CarBehaviour.cs
│   │   ├── CarManager.cs
│   │   ├── DrivingSurfaceManager.cs
│   │   ├── LightEstimation.cs
│   │   ├── PackageBehaviour.cs
│   │   ├── ReticleBehaviour.cs
│   │   ├── ScoreUI.cs                     # (propio) puntuación
│   │   ├── Spawner.cs                     # (modificado) spawner genérico con Tag
│   │   ├── Car Prefab.prefab
│   │   ├── Package Prefab.prefab
│   │   ├── Reticle Prefab.prefab
│   │   ├── Driving Surface Plane.prefab
│   │   └── third_party/                   # Feathered Plane y textura Ground026
│   ├── Spiders/                           # Asset Fuga Spiders
│   │   └── Prefabs/                       # Black Widow, Sand Spider
│   ├── XR/                                # Configuración ARCore / simulación
│   ├── Settings/                          # Configuración URP
│   └── TextMesh Pro/
├── Packages/
└── ProjectSettings/
```

## Scripts

| Script | Función |
|--------|---------|
| `ReticleBehaviour` | Lanza cada frame un *raycast* desde el centro de la pantalla contra los planos detectados y coloca la retícula en el punto de impacto. Si hay un plano bloqueado, solo acepta impactos sobre él. |
| `DrivingSurfaceManager` | Guarda el `ARPlaneManager` y el `ARRaycastManager`. `LockPlane` desactiva el resto de planos y los que se detecten después, dejando solo el plano de juego. |
| `CarManager` | Al tocar la pantalla (y si la retícula está sobre un plano) instancia el coche en la retícula y bloquea ese plano. |
| `CarBehaviour` | El coche se orienta y avanza hacia la retícula. En `OnTriggerEnter` destruye cualquier objeto con `PackageBehaviour` (paquetes y arañas). |
| `PackageBehaviour` | Componente marcador vacío para identificar los objetos recogibles. |
| `Spawner` | Genera un objeto en un punto aleatorio del plano bloqueado, le asigna el `Tag` configurado y, cuando desaparece, genera otro. |
| `ScoreUI` | Suma o resta puntos según el tag del objeto tocado y actualiza el texto en pantalla. |
| `LightEstimation` | Ajusta la luz de la escena con la estimación de luz de ARCore. |

### Mecánica de puntos (`ScoreUI`)

El script va en el prefab del coche. Al arrancar busca en la escena el objeto con tag `ScoreText` y, en cada colisión, comprueba el tag del otro objeto:

```csharp
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Package"))
    {
        score++;
        UpdateText();
    }
    else if (other.CompareTag("Spider"))
    {
        score--;
        UpdateText();
    }
}

void UpdateText()
{
    scoreText.text = "Score: " + score;
}
```

### Aparición de objetos (`Spawner`)

El `Spawner` del Codelab se ha hecho reutilizable añadiendo el campo `Tag`. Se usan **dos instancias**: una para paquetes y otra para arañas. En `Update`, si hay plano bloqueado y el objeto actual ya no existe (porque el coche lo destruyó), genera uno nuevo en un triángulo aleatorio de la malla del plano:

```csharp
public void SpawnPackage(ARPlane plane)
{
    var packageClone = GameObject.Instantiate(PackagePrefab);
    packageClone.transform.position = FindRandomLocation(plane);

    packageClone.tag = Tag;   // "Package" o "Spider" según el spawner
    Package = packageClone.GetComponent<PackageBehaviour>();
}
```

El campo `PackagePrefab` se reutiliza para la araña, por lo que **el prefab de la araña debe llevar el componente `PackageBehaviour`**; si no, `GetComponent` devuelve `null` y el spawner no detecta cuándo reaparecer.

### Flujo de una colisión

```
Coche entra en el trigger
  ├─ CarBehaviour.OnTriggerEnter → Destroy(objeto)
  └─ ScoreUI.OnTriggerEnter      → score ±1 según el tag
Spawner.Update detecta Package == null → genera otro objeto del mismo tipo
```

## Configuración de la escena y los prefabs

**Tags** (*Tags & Layers*): `Package`, `Spider` y `ScoreText`.

**Prefab de la araña** (a partir de `Black Widow` o `Sand Spider`, de `Assets/Spiders/Prefabs`):
- `Collider` con **Is Trigger** activado.
- Componente `PackageBehaviour`.
- Escala ajustada al tamaño del plano y del coche.

**Prefab del paquete** (`Package Prefab`): `Collider` en modo trigger y `PackageBehaviour`.

**Prefab del coche** (`Car Prefab`): `CarBehaviour`, `ScoreUI` y un `Rigidbody`, necesario para que se disparen los eventos de trigger.

**Escena:**
- Dos objetos `Spawner`, ambos con la referencia a `DrivingSurfaceManager`:
  - Spawner de paquetes: `PackagePrefab` = paquete, `Tag` = `Package`.
  - Spawner de arañas: `PackagePrefab` = araña, `Tag` = `Spider`.
- Un texto **TextMeshPro** en un `Canvas` con el tag `ScoreText`, que muestra la puntuación.
- `CarManager` con las referencias al prefab del coche, la retícula y el `DrivingSurfaceManager`.
- Luz direccional con `LightEstimation` y referencia al `ARCameraManager`.

## Cómo jugar

1. Inicia la app y mueve el móvil despacio para que ARCore detecte superficies.
2. Apunta al suelo o a una mesa: la retícula aparece sobre el plano detectado.
3. **Toca la pantalla** para colocar el coche. Ese plano queda bloqueado como zona de juego.
4. El coche **persigue la retícula**: muévela desplazando el móvil.
5. Recoge los **paquetes** (+1) y evita las **arañas** (−1). Cada objeto recogido reaparece en otro punto del plano.
6. La puntuación se muestra en pantalla.

## Créditos y referencias

- Scripts base (`CarBehaviour`, `CarManager`, `DrivingSurfaceManager`, `LightEstimation`, `PackageBehaviour`, `ReticleBehaviour`, `Spawner`): © 2021 Google LLC, licencia Apache 2.0, del [Codelab de AR Foundation](https://codelabs.developers.google.com/arcore-unity-ar-foundation#0). `Spawner` ha sido modificado para admitir el campo `Tag`.
- `ScoreUI`: desarrollo propio.
- Modelo de araña: [Fuga Spiders with destructible eggs and mummy](https://assetstore.unity.com/packages/3d/characters/creatures/fuga-spiders-with-destructible-eggs-and-mummy-151921).
- Textura `Ground026` y *Unity Feathered Plane*: ver los archivos `LICENSE` y `ATTRIBUTION` en `Assets/Starter Package/third_party/`.
- [Documentación de AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest) · [Dispositivos compatibles con ARCore](https://developers.google.com/ar/devices)
- Existe un [ejemplo para iOS (ARKit)](https://medium.com/augmented-reality-tutorial/day-1-creating-an-ar-app-for-ios-using-unity-and-arkit-tutorials-for-ar-e99d5a2e8c8b), **no verificado** en esta práctica.
