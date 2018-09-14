require "BaseParser"

Class.XMLParser = {
	__inherit = "BaseParser",
}

function XMLParser:ParseXML(text, ...)
	local status, result = pcall(self.__ParseXML, self, text, ...)
	if status then
		return result
	else
		print("[XMLParser] Error parsing XML", result)
	end
end

function XMLParser:__ParseXML(text)
	if not text then return end

	local elements = { }

	local i = 1
	local len = #text

	while true do
		if i > len then break end

		local tag
		tag, i = self:ParseOpeningTag(text, i)
		if tag then
			if single_line then
				table.insert(elements, tag)
			else
				local content
				content, i = self:ParseContentAndClosingTag(text, tag.tag, i)
				if text then
					tag.content = content and string.trim_spaces(content) or ""
					table.insert(elements, tag)
					elements[tag.tag] = tag
				end
			end
		else
			local chr = string.sub(text, i, i)
			if type(elements[#elements]) ~= "string" then
				elements[#elements + 1] = ""
			end
			elements[#elements] = elements[#elements] .. chr
		end

		i = i + 1
	end


	return elements	
end

local function IsWhitespace(chr)
	return chr == " " or chr == "\n" or chr == "\t"
end

local function IsLetter(chr)
	local a, z, A, Z = string.byte("azAZ", 1, 4)
	local n = string.byte(chr)
	return (a <= n and n <= z) or (A <= n and n <= Z)
end

local function IsDigit(chr)
	local zero, nine = string.byte("09", 1, 2)
	local n = string.byte(chr)
	return (zero <= n and n <= nine)
end

local function IsLetterDigit(chr)
	return IsLetter(chr) or IsDigit(chr)
end

local function IsSeparator(chr)
	return chr == ">" or chr == "/" or chr == "\"" or chr == "="
end

function XMLParser:ParseXMLWord(text, first)
	local word = ""

	local i = first
	local len = #text

	while true do
		if i > len then return false, first end
		local chr = string.sub(text, i, i)
		if IsWhitespace(chr) then break end
		if IsSeparator(chr) then break end

		word = word .. chr

		i = i + 1
	end

	if #word == 0 then
		return false, first
	else
		return word, i
	end
end

function XMLParser:ParseXMLAttributes(text, first)
	local attributes = { }

	local i = first
	local len = #text

	i = self:SkipWhitespaces(text, i)
	if i > len then return false, first end

	while true do
		if i > len then return false, first end

		--parse key
		local key
		key, i = self:ParseXMLWord(text, i)
		if not key then break end
		if i > len then return false, first end

		--parse =
		i = self:SkipWhitespaces(text, i)
		if i > len then return false, first end
		local chr = string.sub(text, i, i)
		if chr ~= "=" then return false, first end
		i = i + 1
		if i > len then return false, first end
		i = self:SkipWhitespaces(text, i)
		if i > len then return false, first end

		--parse "
		local chr = string.sub(text, i, i)
		if chr ~= "\"" then return false, first end
		i = i + 1
		if i > len then return false, first end

		--parse value and closing "
		local value = ""
		while true do
			if i > len then return false, first end
			local chr = string.sub(text, i, i)

			--parse closing "
			if chr == "\"" then --TODO implement escaped "
				i = i + 1
				break
			end

			value = value .. chr

			i = i + 1
		end

		attributes[key] = value

		i = self:SkipWhitespaces(text, i)
	end

	return attributes, i
end

function XMLParser:SkipWhitespaces(text, first)
	local i = first
	local len = #text

	while true do
		if i > len then return i end
		local chr = string.sub(text, i, i)
		if not IsWhitespace(chr) then break end

		i = i + 1
	end

	return i
end

function XMLParser:ParseOpeningTag(text, first)
	local tag, attributes, single_line

	local i = first
	local len = #text

	local chr = string.sub(text, i, i)
	if chr ~= "<" then --parse <
		return false, first
	end
	i = i + 1
	if i > len then return false, first end

	tag, i = self:ParseXMLWord(text, i) --parse tag
	if not tag then return false, first end
	i = self:SkipWhitespaces(text, i)
	if i > len then return false, first end

	local attributes
	attributes, i = self:ParseXMLAttributes(text, i) --parse all attributes
	if not attributes then return false, first end
	i = self:SkipWhitespaces(text, i)
	if i > len then return false, first end

	local chr = string.sub(text, i, i)
	if chr == "/" then --try parse single line tag /
		single_line = true
		i = i + 1
		if i > len then return false, first end
		i = self:SkipWhitespaces(text, i)
		if i > len then return false, first end
	end

	local chr = string.sub(text, i, i)
	if chr ~= ">" then --parse >
		return false, first
	end
	i = i + 1

	local result = { tag = tag, attributes = attributes, single_line = single_line }
	return result, i
end

function XMLParser:ParseContentAndClosingTag(text, tag, first)
	local content = ""

	local i = first
	local len = #text

	while true do
		if i > len then return false, first end

		local closed
		closed, i = self:ParseClosingTag(text, tag, i)
		if closed then
			break
		else
			local chr = string.sub(text, i, i)
			content = content .. chr
		end

		 i = i + 1
	end

	return content, i
end

function XMLParser:ParseClosingTag(text, tag, first)
	local i = first
	local len = #text

	if string.sub(text, i, i) ~= "<" then --parse <
		return false, first
	end
	i = i + 1
	if i > len then return false, first end
	i = self:SkipWhitespaces(text, i)

	chr = string.sub(text, i, i) --parse closing tag /
	if chr ~= "/" then
		return false, first
	end
	i = i + 1
	if i > len then return false, first end
	i = self:SkipWhitespaces(text, i)
	if i > len then return false, first end

	local closing_tag
	closing_tag, i = self:ParseXMLWord(text, i) --parse tag
	if closing_tag ~= tag then
		return false, first
	end
	i = self:SkipWhitespaces(text, i)
	if i > len then return false, first end

	if string.sub(text, i, i) ~= ">" then --parse >
		return false, first
	end
	i = i + 1

	return "closed", i
end

