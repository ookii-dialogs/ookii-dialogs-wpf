﻿#region Copyright 2009-2021 Ookii Dialogs Contributors
//
// Licensed under the BSD 3-Clause License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;

namespace Ookii.Dialogs.Wpf
{
    /// <summary>
    /// The exception that is thrown when an error occurs getting credentials.
    /// </summary>
    /// <threadsafety instance="false" static="true" />
    [Serializable()]
    public class CredentialException : System.ComponentModel.Win32Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialException" /> class.
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CredentialException()
            : base(Properties.Resources.CredentialError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialException" /> class with the specified error. 
        /// </summary>
        /// <param name="error">The Win32 error code associated with this exception.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CredentialException(int error)
            : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CredentialException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialException" /> class with the specified error and the specified detailed description.
        /// </summary>
        /// <param name="error">The Win32 error code associated with this exception.</param>
        /// <param name="message">A detailed description of the error.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CredentialException(int error, string message)
            : base(error, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">A reference to the inner exception that is the cause of the current exception.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public CredentialException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected CredentialException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

}
