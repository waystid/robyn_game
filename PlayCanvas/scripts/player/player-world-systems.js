// ============================================================================
// PlayCanvas Player & World Systems
// Converted from Unity C# to JavaScript
// ============================================================================

// ----------------------------------------------------------------------------
// PLAYER CONTROLLER - Character movement and interaction
// Converted from Unity PlayerController.cs
// ----------------------------------------------------------------------------
var PlayerController = pc.createScript('playerController');

PlayerController.attributes.add('speed', {
    type: 'number',
    default: 5,
    description: 'Movement speed'
});

PlayerController.attributes.add('runMultiplier', {
    type: 'number',
    default: 1.5,
    description: 'Speed multiplier when running'
});

PlayerController.attributes.add('camera', {
    type: 'entity',
    description: 'Camera to follow player'
});

PlayerController.prototype.initialize = function() {
    this.moveDir = new pc.Vec3();
    this.isRunning = false;
    this.currentSpeed = this.speed;

    // Stats
    this.stats = {
        health: 100,
        maxHealth: 100,
        mana: 100,
        maxMana: 100,
        stamina: 100,
        maxStamina: 100,
        level: 1,
        experience: 0
    };

    console.log('[PlayerController] Initialized');
};

PlayerController.prototype.update = function(dt) {
    this.handleInput();
    this.movePlayer(dt);
    this.updateCamera();
};

PlayerController.prototype.handleInput = function() {
    this.moveDir.set(0, 0, 0);

    // WASD movement
    if (this.app.keyboard.isPressed(pc.KEY_W) || this.app.keyboard.isPressed(pc.KEY_UP)) {
        this.moveDir.z -= 1;
    }
    if (this.app.keyboard.isPressed(pc.KEY_S) || this.app.keyboard.isPressed(pc.KEY_DOWN)) {
        this.moveDir.z += 1;
    }
    if (this.app.keyboard.isPressed(pc.KEY_A) || this.app.keyboard.isPressed(pc.KEY_LEFT)) {
        this.moveDir.x -= 1;
    }
    if (this.app.keyboard.isPressed(pc.KEY_D) || this.app.keyboard.isPressed(pc.KEY_RIGHT)) {
        this.moveDir.x += 1;
    }

    // Running with Shift
    this.isRunning = this.app.keyboard.isPressed(pc.KEY_SHIFT);
    this.currentSpeed = this.isRunning ? this.speed * this.runMultiplier : this.speed;
};

PlayerController.prototype.movePlayer = function(dt) {
    if (this.moveDir.length() > 0) {
        this.moveDir.normalize();

        var pos = this.entity.getPosition();
        pos.x += this.moveDir.x * this.currentSpeed * dt;
        pos.z += this.moveDir.z * this.currentSpeed * dt;
        this.entity.setPosition(pos);

        // Rotate to face movement direction
        if (this.moveDir.length() > 0.1) {
            var angle = Math.atan2(this.moveDir.x, this.moveDir.z) * pc.math.RAD_TO_DEG;
            this.entity.setEulerAngles(0, angle, 0);
        }
    }
};

PlayerController.prototype.updateCamera = function() {
    if (!this.camera) return;

    var playerPos = this.entity.getPosition();
    var camPos = this.camera.getPosition();

    // Smooth camera follow
    camPos.x = pc.math.lerp(camPos.x, playerPos.x, 0.1);
    camPos.z = pc.math.lerp(camPos.z, playerPos.z + 10, 0.1);

    this.camera.setPosition(camPos);
};

PlayerController.prototype.takeDamage = function(amount) {
    this.stats.health -= amount;
    if (this.stats.health < 0) this.stats.health = 0;

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('player:damaged', amount, this.stats.health);
    }

    if (this.stats.health <= 0) {
        this.die();
    }
};

PlayerController.prototype.heal = function(amount) {
    this.stats.health += amount;
    if (this.stats.health > this.stats.maxHealth) {
        this.stats.health = this.stats.maxHealth;
    }

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('player:healed', amount, this.stats.health);
    }
};

PlayerController.prototype.die = function() {
    console.log('[PlayerController] Player died');

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('player:died');
    }
};

PlayerController.prototype.getSaveData = function() {
    return {
        position: {
            x: this.entity.getPosition().x,
            y: this.entity.getPosition().y,
            z: this.entity.getPosition().z
        },
        stats: this.stats
    };
};

PlayerController.prototype.loadSaveData = function(data) {
    if (data.position) {
        this.entity.setPosition(data.position.x, data.position.y, data.position.z);
    }
    if (data.stats) {
        this.stats = data.stats;
    }
};


// ----------------------------------------------------------------------------
// TIME MANAGER - Day/night cycle and time progression
// Converted from Unity TimeManager.cs
// ----------------------------------------------------------------------------
var TimeManager = pc.createScript('timeManager');

TimeManager.getInstance = function() {
    return TimeManager._instance;
};

TimeManager.attributes.add('timeScale', {
    type: 'number',
    default: 60,
    description: 'How many game minutes pass per real second'
});

TimeManager.attributes.add('startHour', {
    type: 'number',
    default: 6,
    description: 'Starting hour (0-23)'
});

TimeManager.attributes.add('directionalLight', {
    type: 'entity',
    description: 'Directional light for sun'
});

TimeManager.prototype.initialize = function() {
    TimeManager._instance = this;

    // Time state
    this.currentDay = 1;
    this.currentHour = this.startHour;
    this.currentMinute = 0;
    this.timeOfDay = this.currentHour + (this.currentMinute / 60);

    // Seasons
    this.currentSeason = 'spring'; // spring, summer, autumn, winter
    this.dayOfSeason = 1;
    this.daysPerSeason = 28;

    // Paused
    this.isPaused = false;

    console.log('[TimeManager] Initialized - Day ' + this.currentDay + ', ' + this.getTimeString());

    this.updateDayNightCycle();
};

TimeManager.prototype.update = function(dt) {
    if (this.isPaused) return;

    // Advance time
    var minutesAdvanced = (dt * this.timeScale);
    this.currentMinute += minutesAdvanced;

    // Handle minute overflow
    if (this.currentMinute >= 60) {
        var hours = Math.floor(this.currentMinute / 60);
        this.currentMinute = this.currentMinute % 60;
        this.advanceHours(hours);
    }

    // Update time of day
    this.timeOfDay = this.currentHour + (this.currentMinute / 60);

    // Update visual cycle
    this.updateDayNightCycle();
};

TimeManager.prototype.advanceHours = function(hours) {
    this.currentHour += hours;

    if (this.currentHour >= 24) {
        var days = Math.floor(this.currentHour / 24);
        this.currentHour = this.currentHour % 24;
        this.advanceDays(days);
    }

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('time:hourChanged', this.currentHour);
    }
};

TimeManager.prototype.advanceDays = function(days) {
    this.currentDay += days;
    this.dayOfSeason += days;

    if (this.dayOfSeason > this.daysPerSeason) {
        this.dayOfSeason = 1;
        this.advanceSeason();
    }

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('time:dayChanged', this.currentDay);
    }

    console.log('[TimeManager] New day: Day ' + this.currentDay);
};

TimeManager.prototype.advanceSeason = function() {
    var seasons = ['spring', 'summer', 'autumn', 'winter'];
    var currentIndex = seasons.indexOf(this.currentSeason);
    this.currentSeason = seasons[(currentIndex + 1) % seasons.length];

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('time:seasonChanged', this.currentSeason);
    }

    console.log('[TimeManager] New season: ' + this.currentSeason);
};

TimeManager.prototype.updateDayNightCycle = function() {
    if (!this.directionalLight) return;

    // Calculate sun angle based on time (0-24)
    // 6:00 = sunrise (0°)
    // 12:00 = noon (90°)
    // 18:00 = sunset (180°)
    // 0:00 = midnight (270°)

    var sunAngle = ((this.timeOfDay - 6) / 24) * 360;
    this.directionalLight.setEulerAngles(sunAngle, 0, 0);

    // Calculate light intensity (brighter during day)
    var intensity;
    if (this.timeOfDay >= 6 && this.timeOfDay <= 18) {
        // Day time
        var dayProgress = (this.timeOfDay - 6) / 12;
        intensity = 0.5 + Math.sin(dayProgress * Math.PI) * 0.5; // 0.5 to 1.0
    } else {
        // Night time
        intensity = 0.1;
    }

    var light = this.directionalLight.light;
    if (light) {
        light.intensity = intensity;
    }
};

TimeManager.prototype.getTimeString = function() {
    var hour = Math.floor(this.currentHour);
    var minute = Math.floor(this.currentMinute);
    var hourStr = (hour < 10 ? '0' : '') + hour;
    var minuteStr = (minute < 10 ? '0' : '') + minute;
    return hourStr + ':' + minuteStr;
};

TimeManager.prototype.isNight = function() {
    return this.timeOfDay < 6 || this.timeOfDay >= 18;
};

TimeManager.prototype.isDay = function() {
    return !this.isNight();
};

TimeManager.prototype.getSaveData = function() {
    return {
        currentDay: this.currentDay,
        currentHour: this.currentHour,
        currentMinute: this.currentMinute,
        currentSeason: this.currentSeason,
        dayOfSeason: this.dayOfSeason
    };
};

TimeManager.prototype.loadSaveData = function(data) {
    this.currentDay = data.currentDay || 1;
    this.currentHour = data.currentHour || 6;
    this.currentMinute = data.currentMinute || 0;
    this.currentSeason = data.currentSeason || 'spring';
    this.dayOfSeason = data.dayOfSeason || 1;
    this.timeOfDay = this.currentHour + (this.currentMinute / 60);

    this.updateDayNightCycle();
};


// ----------------------------------------------------------------------------
// WEATHER SYSTEM - Dynamic weather and effects
// Converted from Unity WeatherSystem.cs
// ----------------------------------------------------------------------------
var WeatherSystem = pc.createScript('weatherSystem');

WeatherSystem.getInstance = function() {
    return WeatherSystem._instance;
};

WeatherSystem.prototype.initialize = function() {
    WeatherSystem._instance = this;

    // Weather types
    this.weatherTypes = {
        clear: { name: 'Clear', fogDensity: 0, rainIntensity: 0 },
        cloudy: { name: 'Cloudy', fogDensity: 0.001, rainIntensity: 0 },
        rain: { name: 'Rain', fogDensity: 0.002, rainIntensity: 0.5 },
        storm: { name: 'Storm', fogDensity: 0.003, rainIntensity: 1.0 },
        snow: { name: 'Snow', fogDensity: 0.0015, rainIntensity: 0.3 }
    };

    // Current weather
    this.currentWeather = 'clear';
    this.targetWeather = 'clear';
    this.transitionProgress = 1;
    this.transitionDuration = 5; // seconds

    // Random weather changes
    this.weatherChangeInterval = 300; // 5 minutes
    this.timeSinceLastChange = 0;

    console.log('[WeatherSystem] Initialized');
};

WeatherSystem.prototype.update = function(dt) {
    // Handle weather transition
    if (this.transitionProgress < 1) {
        this.transitionProgress += dt / this.transitionDuration;
        if (this.transitionProgress >= 1) {
            this.transitionProgress = 1;
            this.currentWeather = this.targetWeather;
        }
        this.applyWeatherEffects();
    }

    // Random weather changes
    this.timeSinceLastChange += dt;
    if (this.timeSinceLastChange >= this.weatherChangeInterval) {
        this.changeToRandomWeather();
        this.timeSinceLastChange = 0;
    }
};

WeatherSystem.prototype.changeWeather = function(weatherType) {
    if (!this.weatherTypes[weatherType]) {
        console.error('[WeatherSystem] Invalid weather type:', weatherType);
        return;
    }

    this.targetWeather = weatherType;
    this.transitionProgress = 0;

    console.log('[WeatherSystem] Changing to ' + weatherType);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('weather:changing', weatherType);
    }
};

WeatherSystem.prototype.changeToRandomWeather = function() {
    var types = Object.keys(this.weatherTypes);
    var randomType = types[Math.floor(Math.random() * types.length)];
    this.changeWeather(randomType);
};

WeatherSystem.prototype.applyWeatherEffects = function() {
    var current = this.weatherTypes[this.currentWeather];
    var target = this.weatherTypes[this.targetWeather];

    // Lerp between current and target weather
    var t = this.transitionProgress;
    var fogDensity = pc.math.lerp(current.fogDensity, target.fogDensity, t);
    var rainIntensity = pc.math.lerp(current.rainIntensity, target.rainIntensity, t);

    // Apply fog (if scene supports it)
    // Apply rain particle effects (if available)

    // You would apply these to your scene's fog and particle systems here
};

WeatherSystem.prototype.getCurrentWeather = function() {
    return this.currentWeather;
};

WeatherSystem.prototype.getSaveData = function() {
    return {
        currentWeather: this.currentWeather
    };
};

WeatherSystem.prototype.loadSaveData = function(data) {
    if (data.currentWeather) {
        this.currentWeather = data.currentWeather;
        this.targetWeather = data.currentWeather;
        this.transitionProgress = 1;
        this.applyWeatherEffects();
    }
};


// ============================================================================
// VALIDATION
// ============================================================================

console.log('=== PlayCanvas Player & World Systems Loaded ===');
console.log('- PlayerController');
console.log('- TimeManager');
console.log('- WeatherSystem');
console.log('===========================================');
