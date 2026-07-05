# ¡Qué Más Parcero!

Juego de plataformas 2D estilo *endless runner* con temática colombiana, desarrollado en **Unity** y **C#**. Controlas a un frijol que corre automáticamente, esquiva enemigos y recolecta empanadas mientras el nivel se genera proceduralmente.

![Unity](https://img.shields.io/badge/Unity-2021.1-black?logo=unity)
![C%23](https://img.shields.io/badge/C%23-Scripts-239120?logo=csharp)

## 🎮 Cómo jugar

| Acción | Tecla |
|---|---|
| Empezar partida | `Enter` |
| Saltar | `Espacio` |
| Super salto (gasta maná) | ver Input Manager (`SuperJump`) |

- Recolecta **empanadas** para sumar puntos.
- Los **enemigos** te quitan vida al tocarte; las pociones la recuperan.
- Si te caes al vacío, pierdes. Tu **récord de distancia** se guarda entre sesiones.

## 🛠️ Características técnicas

- **Generación procedural de nivel**: bloques prefabricados que se instancian y destruyen dinámicamente a medida que el jugador avanza.
- **Máquina de estados de juego** (menú / en juego / game over) con un `GameManager` singleton.
- **Física 2D** con Rigidbody2D, raycast para detección de suelo y capas de colisión.
- **Sistema de vida y maná** con barras de UI y super salto con costo de maná.
- **Persistencia** del puntaje máximo con `PlayerPrefs`.
- **Cámara con seguimiento suavizado** (`SmoothDamp`).

## 🚀 Cómo ejecutarlo

1. Clona el repositorio.
2. Ábrelo con **Unity Hub** (versión 2021.3 LTS o superior; el proyecto se actualiza automáticamente).
3. Abre la escena `Assets/Scenes/GameScene.unity` y presiona ▶️ Play.

## 📁 Estructura

```
Assets/
├── Animations/   # Animaciones y controllers (jugador, enemigos, monedas)
├── Audio/        # Música y efectos de sonido
├── Prefabs/      # Bloques de nivel, jugador, enemigos, coleccionables
├── Scenes/       # Escena principal del juego
├── Scripts/      # Lógica del juego en C#
└── Sprites/      # Arte 2D
```
