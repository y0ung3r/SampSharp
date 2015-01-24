﻿// SampSharp
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Natives;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.SAMP;

namespace SampSharp.GameMode.World
{
    /// <summary>
    ///     Represents a player-object.
    /// </summary>
    public class PlayerObject : IdentifiedOwnedPool<PlayerObject>, IGameObject, IOwnable<GtaPlayer>, IIdentifiable
    {
        #region Fields

        /// <summary>
        ///     The invalid identifier.
        /// </summary>
        public const int InvalidId = Misc.InvalidObjectId;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the rotation of this IGameObject.
        /// </summary>
        public virtual Vector Rotation
        {
            get { return Native.GetPlayerObjectRot(Owner.Id, Id); }
            set { Native.SetPlayerObjectRot(Owner.Id, Id, value); }
        }

        /// <summary>
        ///     Gets the position of this IWorldObject.
        /// </summary>
        public virtual Vector Position
        {
            get { return Native.GetPlayerObjectPos(Owner.Id, Id); }
            set { Native.SetPlayerObjectPos(Owner.Id, Id, value); }
        }

        /// <summary>
        ///     Gets whether this IGameObject is moving.
        /// </summary>
        public virtual bool IsMoving
        {
            get { return Native.IsPlayerObjectMoving(Owner.Id, Id); }
        }

        /// <summary>
        ///     Gets whether this IGameObject is valid.
        /// </summary>
        public virtual bool IsValid
        {
            get { return Native.IsValidPlayerObject(Owner.Id, Id); }
        }

        /// <summary>
        ///     Gets the model of this IGameObject.
        /// </summary>
        public virtual int ModelId { get; private set; }

        /// <summary>
        ///     Gets the draw distance of this IGameObject.
        /// </summary>
        public virtual float DrawDistance { get; private set; }

        /// <summary>
        ///     Gets the Identity of this instance.
        /// </summary>
        public virtual int Id { get; private set; }

        /// <summary>
        ///     Gets the owner of this IOwnable.
        /// </summary>
        public virtual GtaPlayer Owner { get; private set; }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the <see cref="BaseMode.OnObjectMoved" /> callback is being called.
        ///     This callback is called when an object is moved after <see cref="Move(Vector,float)" /> (when it stops moving).
        /// </summary>
        public event EventHandler<PlayerObjectEventArgs> Moved;

        /// <summary>
        ///     Occurs when the <see cref="BaseMode.OnPlayerSelectObject" /> callback is being called.
        ///     This callback is called when a player selects an object after <see cref="Native.SelectObject" /> has been used.
        /// </summary>
        public event EventHandler<PlayerSelectObjectEventArgs> Selected;

        /// <summary>
        ///     Occurs when the <see cref="BaseMode.OnPlayerEditObject" /> callback is being called.
        ///     This callback is called when a player ends object edition mode.
        /// </summary>
        public event EventHandler<PlayerEditObjectEventArgs> Edited;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public PlayerObject(int id) : this(GtaPlayer.Find(id/(Limits.MaxObjects + 1)), id%(Limits.MaxObjects + 1))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.ArgumentNullException">owner</exception>
        public PlayerObject(GtaPlayer owner, int id)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            Id = id;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="modelid">The modelid.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        public PlayerObject(GtaPlayer owner, int modelid, Vector position, Vector rotation)
            : this(owner, modelid, position, rotation, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerObject" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="modelid">The modelid.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="drawDistance">The draw distance.</param>
        /// <exception cref="System.ArgumentNullException">owner</exception>
        public PlayerObject(GtaPlayer owner, int modelid, Vector position, Vector rotation, float drawDistance)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            ModelId = modelid;
            DrawDistance = drawDistance;

            Id = Native.CreatePlayerObject(owner.Id, modelid, position, rotation, drawDistance);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Moves this IGameObject to the given position and rotation with the given speed.
        /// </summary>
        /// <param name="position">The position to which to move this IGameObject.</param>
        /// <param name="speed">The speed at which to move this IGameObject.</param>
        /// <param name="rotation">The rotation to which to move this IGameObject.</param>
        /// <returns>
        ///     The time it will take for the object to move in milliseconds.
        /// </returns>
        public virtual int Move(Vector position, float speed, Vector rotation)
        {
            CheckDisposure();

            return Native.MovePlayerObject(Owner.Id, Id, position, speed, rotation);
        }

        /// <summary>
        ///     Moves this IGameObject to the given position with the given speed.
        /// </summary>
        /// <param name="position">The position to which to move this IGameObject.</param>
        /// <param name="speed">The speed at which to move this IGameObject.</param>
        /// <returns>
        ///     The time it will take for the object to move in milliseconds.
        /// </returns>
        public virtual int Move(Vector position, float speed)
        {
            CheckDisposure();

            return Native.MovePlayerObject(Owner.Id, Id, position.X, position.Y, position.Z, speed, -1000,
                -1000, -1000);
        }

        /// <summary>
        ///     Stop this IGameObject from moving any further.
        /// </summary>
        public virtual void Stop()
        {
            CheckDisposure();

            Native.StopPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Sets the material of this IGameObject.
        /// </summary>
        /// <param name="materialindex">The material index on the object to change.</param>
        /// <param name="modelid">
        ///     The modelid on which the replacement texture is located. Use 0 for alpha. Use -1 to change the
        ///     material color without altering the texture.
        /// </param>
        /// <param name="txdname">The name of the txd file which contains the replacement texture (use "none" if not required).</param>
        /// <param name="texturename">The name of the texture to use as the replacement (use "none" if not required).</param>
        /// <param name="materialcolor">The object color to set (use default(Color) to keep the existing material color).</param>
        public virtual void SetMaterial(int materialindex, int modelid, string txdname, string texturename,
            Color materialcolor)
        {
            CheckDisposure();

            Native.SetPlayerObjectMaterial(Owner.Id, Id, materialindex, modelid, txdname, texturename,
                materialcolor.GetColorValue(ColorFormat.ARGB));
        }

        /// <summary>
        ///     Sets the material text of this IGameObject.
        /// </summary>
        /// <param name="materialindex">The material index on the object to change.</param>
        /// <param name="text">The text to show on the object. (MAX 2048 characters)</param>
        /// <param name="materialsize">The object's material index to replace with text.</param>
        /// <param name="fontface">The font to use.</param>
        /// <param name="fontsize">The size of the text (max 255).</param>
        /// <param name="bold">Whether to write in bold.</param>
        /// <param name="foreColor">The color of the text.</param>
        /// <param name="backColor">The background color of the text.</param>
        /// <param name="textalignment">The alignment of the text.</param>
        public virtual void SetMaterialText(int materialindex, string text, ObjectMaterialSize materialsize,
            string fontface, int fontsize, bool bold, Color foreColor, Color backColor,
            ObjectMaterialTextAlign textalignment)
        {
            CheckDisposure();

            Native.SetPlayerObjectMaterialText(Owner.Id, Id, text, materialindex, (int) materialsize,
                fontface, fontsize, bold,
                foreColor.GetColorValue(ColorFormat.ARGB), backColor.GetColorValue(ColorFormat.ARGB),
                (int) textalignment);
        }

        /// <summary>
        ///     Attaches this <see cref="PlayerObject" /> to the specified <paramref name="player" />.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotation">The rotation.</param>
        /// <exception cref="System.ArgumentNullException">player</exception>
        public virtual void AttachTo(GtaPlayer player, Vector offset, Vector rotation)
        {
            CheckDisposure();

            if (player == null)
                throw new ArgumentNullException("player");

            Native.AttachPlayerObjectToPlayer(Owner.Id, Id, player.Id, offset, rotation);
        }

        /// <summary>
        ///     Attaches this <see cref="PlayerObject" /> to the specified <paramref name="vehicle" />.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="rotation">The rotation.</param>
        /// <exception cref="System.ArgumentNullException">vehicle</exception>
        public virtual void AttachTo(GtaVehicle vehicle, Vector offset, Vector rotation)
        {
            CheckDisposure();

            if (vehicle == null)
                throw new ArgumentNullException("vehicle");

            Native.AttachPlayerObjectToVehicle(Owner.Id, Id, vehicle.Id, offset, rotation);
        }

        /// <summary>
        ///     Attaches the player's camera to this PlayerObject.
        /// </summary>
        /// <remarks>
        ///     This will attach the camera of the player whose object this is to this object.
        /// </remarks>
        public virtual void AttachCameraToObject()
        {
            CheckDisposure();

            Native.AttachCameraToPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Performs tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether managed resources should be disposed.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Native.DestroyPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Lets the <see cref="Owner" /> of this <see cref="PlayerObject" /> edit this object.
        /// </summary>
        public virtual void Edit()
        {
            CheckDisposure();

            Native.EditPlayerObject(Owner.Id, Id);
        }

        /// <summary>
        ///     Lets the specified <paramref name="player" /> select an object.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <exception cref="System.ArgumentNullException">player</exception>
        public static void Select(GtaPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            Native.SelectObject(player.Id);
        }

        #endregion

        #region Event raisers

        /// <summary>
        ///     Raises the <see cref="Moved" /> event.
        /// </summary>
        /// <param name="e">An <see cref="ObjectEventArgs" /> that contains the event data. </param>
        public virtual void OnMoved(PlayerObjectEventArgs e)
        {
            if (Moved != null)
                Moved(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Selected" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerSelectObjectEventArgs" /> that contains the event data. </param>
        public virtual void OnSelected(PlayerSelectObjectEventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="Edited" /> event.
        /// </summary>
        /// <param name="e">An <see cref="PlayerEditObjectEventArgs" /> that contains the event data. </param>
        public virtual void OnEdited(PlayerEditObjectEventArgs e)
        {
            if (Edited != null)
                Edited(this, e);
        }

        #endregion
    }
}