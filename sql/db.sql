-- phpMyAdmin SQL Dump
-- version 4.9.0.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 23-08-2019 a las 14:02:19
-- Versión del servidor: 10.3.16-MariaDB
-- Versión de PHP: 7.3.7

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `hitssniffer`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `hit_counter`
--

CREATE TABLE `hit_counter` (
  `id` int(11) NOT NULL,
  `repo_id` int(11) DEFAULT NULL,
  `date` datetime NOT NULL,
  `path` text NOT NULL,
  `hits` int(11) NOT NULL,
  `hash` text NOT NULL,
  `sid` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `organizations`
--

CREATE TABLE `organizations` (
  `id` int(11) NOT NULL,
  `name` text NOT NULL,
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `organization_stats`
--

CREATE TABLE `organization_stats` (
  `id` int(11) NOT NULL,
  `org_id` int(11) NOT NULL,
  `name` text NOT NULL,
  `date` datetime NOT NULL,
  `members` int(11) NOT NULL,
  `repositories` int(11) NOT NULL,
  `packages` int(11) NOT NULL,
  `teams` int(11) NOT NULL,
  `projects` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `repositories`
--

CREATE TABLE `repositories` (
  `id` int(11) NOT NULL,
  `org_id` int(11) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  `name` text NOT NULL,
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `repository_stats`
--

CREATE TABLE `repository_stats` (
  `id` int(11) NOT NULL,
  `org_owner_id` int(11) DEFAULT NULL,
  `user_owner_id` int(11) DEFAULT NULL,
  `name` text NOT NULL,
  `date` datetime NOT NULL,
  `commits` int(11) NOT NULL,
  `branches` int(11) NOT NULL,
  `releases` int(11) NOT NULL,
  `contributors` int(11) NOT NULL,
  `stars` int(11) NOT NULL,
  `forks` int(11) NOT NULL,
  `watchers` int(11) NOT NULL,
  `pulls` int(11) NOT NULL,
  `projects` int(11) NOT NULL,
  `issues` int(11) NOT NULL,
  `last_commit` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `name` text NOT NULL,
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `user_stats`
--

CREATE TABLE `user_stats` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `name` text NOT NULL,
  `date` datetime NOT NULL,
  `followers` int(11) NOT NULL,
  `following` int(11) NOT NULL,
  `repositories` int(11) NOT NULL,
  `commits` int(11) NOT NULL,
  `commits_last_year` int(11) NOT NULL,
  `projects` int(11) NOT NULL,
  `starts_given` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `hit_counter`
--
ALTER TABLE `hit_counter`
  ADD PRIMARY KEY (`id`),
  ADD KEY `owner_id` (`repo_id`);

--
-- Indices de la tabla `organizations`
--
ALTER TABLE `organizations`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `organization_stats`
--
ALTER TABLE `organization_stats`
  ADD PRIMARY KEY (`id`),
  ADD KEY `org_id` (`org_id`);

--
-- Indices de la tabla `repositories`
--
ALTER TABLE `repositories`
  ADD PRIMARY KEY (`id`),
  ADD KEY `org_id` (`org_id`) USING BTREE,
  ADD KEY `user_id` (`user_id`);

--
-- Indices de la tabla `repository_stats`
--
ALTER TABLE `repository_stats`
  ADD PRIMARY KEY (`id`),
  ADD KEY `org_owner_id` (`org_owner_id`,`user_owner_id`),
  ADD KEY `user_owner_id` (`user_owner_id`);

--
-- Indices de la tabla `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `user_stats`
--
ALTER TABLE `user_stats`
  ADD PRIMARY KEY (`id`),
  ADD KEY `user_id` (`user_id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `hit_counter`
--
ALTER TABLE `hit_counter`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `organizations`
--
ALTER TABLE `organizations`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `organization_stats`
--
ALTER TABLE `organization_stats`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `repositories`
--
ALTER TABLE `repositories`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `repository_stats`
--
ALTER TABLE `repository_stats`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `user_stats`
--
ALTER TABLE `user_stats`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `hit_counter`
--
ALTER TABLE `hit_counter`
  ADD CONSTRAINT `hit_counter_ibfk_1` FOREIGN KEY (`repo_id`) REFERENCES `repositories` (`id`);

--
-- Filtros para la tabla `organizations`
--
ALTER TABLE `organizations`
  ADD CONSTRAINT `organizations_ibfk_1` FOREIGN KEY (`id`) REFERENCES `repositories` (`org_id`);

--
-- Filtros para la tabla `organization_stats`
--
ALTER TABLE `organization_stats`
  ADD CONSTRAINT `organization_stats_ibfk_1` FOREIGN KEY (`id`) REFERENCES `repository_stats` (`org_owner_id`),
  ADD CONSTRAINT `organization_stats_ibfk_2` FOREIGN KEY (`org_id`) REFERENCES `organizations` (`id`);

--
-- Filtros para la tabla `users`
--
ALTER TABLE `users`
  ADD CONSTRAINT `users_ibfk_1` FOREIGN KEY (`id`) REFERENCES `repositories` (`user_id`);

--
-- Filtros para la tabla `user_stats`
--
ALTER TABLE `user_stats`
  ADD CONSTRAINT `user_stats_ibfk_1` FOREIGN KEY (`id`) REFERENCES `repository_stats` (`user_owner_id`),
  ADD CONSTRAINT `user_stats_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
