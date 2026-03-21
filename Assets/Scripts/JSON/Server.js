const express = require('express');
const mongoose = require('mongoose');
const bcrypt = require('bcrypt');
const app = express();
app.use(express.json());

// Connect to MongoDB
mongoose.connect('mongodb://localhost:27017/fusionGame')
  .then(() => console.log('MongoDB connected'))
  .catch(err => console.error('MongoDB error:', err));

// Player schema
const playerSchema = new mongoose.Schema({
  username: { type: String, required: true, unique: true },
  password: { type: String, required: true },
  //cash of the day
  Cash:    { type: Number, default: 0 },
  //Total Earned(Grouped)
  TotalCash: {type: Number, default: 0 }

});
const Player = mongoose.model('Player', playerSchema);

// Register
app.post('/api/player/register', async (req, res) => {
  const { username, password } = req.body;
  if (!username || !password)
    return res.status(400).json({ error: "Username and password required" });

  try {
    const hashed = await bcrypt.hash(password, 10);
    const player = await Player.create({ username, password: hashed });
    res.status(201).json({ message: "Registered successfully", id: player._id });
  } catch (e) {
    if (e.code === 11000)
      return res.status(400).json({ error: "Username already exists" });
    res.status(500).json({ error: "Server error" });
  }
});

// Login
app.post('/api/player/login', async (req, res) => {
  const { username, password } = req.body;
  try {
    const player = await Player.findOne({ username });
    if (!player || !(await bcrypt.compare(password, player.password)))
      return res.status(401).json({ error: "Invalid username or password" });

    res.json({ message: "Login successful", id: player._id, username: player.username });
  } catch {
    res.status(500).json({ error: "Server error" });
  }
});

// Get player
app.get('/api/player/:id', async (req, res) => {
  try {
    const player = await Player.findById(req.params.id);
    if (!player) return res.status(404).json({ error: "Player not found" });

    res.json({ id: player._id, username: player.username, kills: player.kills, deaths: player.deaths });
  } catch {
    res.status(500).json({ error: "Server error" });
  }
});

// Update score
app.put('/api/player/score', async (req, res) => {
  const { id, kills, deaths } = req.body;
  try {
    const player = await Player.findByIdAndUpdate(id, { kills, deaths }, { new: true });
    if (!player) return res.status(404).json({ error: "Player not found" });

    res.json({ message: "Score updated", kills: player.kills, deaths: player.deaths });
  } catch {
    res.status(500).json({ error: "Server error" });
  }
});

// Update password
app.put('/api/player/updatePassword', async (req, res) => {
  const { id, oldPassword, newPassword } = req.body;
  try {
    const player = await Player.findById(id);
    if (!player) return res.status(404).json({ error: "Player not found" });
    if (!(await bcrypt.compare(oldPassword, player.password)))
      return res.status(401).json({ error: "Old password incorrect" });

    player.password = await bcrypt.hash(newPassword, 10);
    await player.save();
    res.json({ message: "Password updated successfully" });
  } catch {
    res.status(500).json({ error: "Server error" });
  }
});

// Delete player
app.delete('/api/player/:id', async (req, res) => {
  try {
    const player = await Player.findByIdAndDelete(req.params.id);
    if (!player) return res.status(404).json({ error: "Player not found" });

    res.json({ message: "Player deleted successfully" });
  } catch {
    res.status(500).json({ error: "Server error" });
  }
});

app.listen(3000, () => console.log('Server running on http://localhost:3000'));