<?php
/*
 * Copyright (C) 2017-2025 CRLibre <https://crlibre.org>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/**
 * Boot up procedure
 */
function clave_bootMeUp()
{
    // Just booting up
}

/**
 * Init function
 */
function clave_init()
{
    $paths = array(
        array(
            'r'             => 'clave',
            'action'        => 'getClave',
            'access'        => 'users_openAccess',
            'access_params' => 'accessName',
            'params'        => array(
                array("key" => "tipoDocumento", "def" => "", "req" => true),
                array("key" => "tipoCedula", "def" => "", "req" => true),
                array("key" => "cedula", "def" => "", "req" => true),
                array("key" => "codigoPais", "def" => "", "req" => true),
                array("key" => "consecutivo", "def" => "", "req" => true),
                array("key" => "situacion", "def" => "", "req" => true),
                array("key" => "terminal", "def" => "", "req" => false),
                array("key" => "sucursal", "def" => "", "req" => false),
                array("key" => "codigoSeguridad", "def" => "", "req" => true)
            ),
            'file'          => 'clave.php'
        )
    );

    return $paths;
}

/* * *********************************************** */
//In the access you can use users_openAccess if you want anyone can use the function
// or users_loggedIn if the user must be logged in
/* * *********************************************** */

/**
 * Get the perms for this module
 */
function MODULENAME_access()
{
    $perms = array(
        array(
            # A human readable name
            'name'          => 'Do something with this module',
            # Something to remember what it is for
            'description'   => 'What can be achieved with this permission',
            # Internal machine name, no spaces, no funny symbols, same rules as a variable
            # Use yourmodule_ prefix
            'code'          => 'mymodule_access_one',
            # Default value in case it is not set
            'def'           => false, //Or true, you decide
        ),
    );
}

/**@}*/
/** @}*/
